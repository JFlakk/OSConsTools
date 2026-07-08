-- =====================================================================================
-- REFERENCE DDL - FMM configurable-construct config tables (application database).
-- One config-driven engine per construct reads these; finance users edit them via
-- SQL Table Editors (no recompile). Mirrors the RMW patterns but externalizes the
-- coordinates/state/mappings that RMW hardcodes in VB.
--
-- Naming: FMM_* config tables + audit columns on every row (Create/Update Date/User),
-- matching the DDM/FMM convention. Swap types/lengths to your standards.
-- =====================================================================================

/* ---------------------------------------------------------------------------
   1) TABLE -> CUBE  (config-driven load; replaces hardcoded Load_Reqs_to_Cube)
   --------------------------------------------------------------------------- */
CREATE TABLE FMM_CubeLoadConfig (
    LoadID              INT           NOT NULL,          -- PK
    Name                NVARCHAR(100) NOT NULL,
    SourceTable         NVARCHAR(200) NOT NULL,          -- e.g. XFC_FMM_Stage
    TargetCube          NVARCHAR(100) NOT NULL,          -- e.g. ARMY  (was hardcoded 'Cb#ARMY')
    TargetScenarioExpr  NVARCHAR(200) NULL,              -- literal or |!token!| resolved at run
    TargetViewExpr      NVARCHAR(50)  NOT NULL DEFAULT 'Periodic',
    OriginExpr          NVARCHAR(50)  NULL,              -- literal/token (was 'Import'/'AdjInput')
    EntityScopeExpr     NVARCHAR(400) NULL,              -- member filter, e.g. E#|!Entity!|.Base
    TimeScopeExpr       NVARCHAR(400) NULL,              -- e.g. T#|!Time!| .. +4 years
    LoadMode            TINYINT       NOT NULL DEFAULT 1,-- 1=FullReplace, 2=Delta
    ClearUnmatched      BIT           NOT NULL DEFAULT 1,-- zero-out cube cells no longer in source
    Status              INT           NOT NULL DEFAULT 1,
    CreateDate DATETIME NULL, CreateUser NVARCHAR(100) NULL,
    UpdateDate DATETIME NULL, UpdateUser NVARCHAR(100) NULL,
    CONSTRAINT PK_FMM_CubeLoadConfig PRIMARY KEY (LoadID)
);

-- Column -> dimension mapping (the piece RMW bakes into VB). This is what makes it configurable.
CREATE TABLE FMM_CubeLoadColMap (
    LoadID           INT           NOT NULL,             -- FK -> FMM_CubeLoadConfig
    SourceColumn     NVARCHAR(128) NOT NULL,             -- staging column name
    TargetDimType    NVARCHAR(30)  NOT NULL,             -- Account|Flow|IC|Origin|UD1..UD8|Entity|Amount
    TargetMemberExpr NVARCHAR(200) NULL,                 -- literal member, {col} to use the row value, or |!token!|
    IsAmount         BIT           NOT NULL DEFAULT 0,   -- 1 => this column is the cell VALUE (UpdateValue)
    Aggregate        NVARCHAR(10)  NULL,                 -- SUM/NONE for the amount column
    SortOrder        INT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_FMM_CubeLoadColMap PRIMARY KEY (LoadID, SourceColumn)
);

-- Delta watermark: only reload POVs whose source rows changed (hash of the slice).
CREATE TABLE FMM_CubeLoadWatermark (
    LoadID      INT           NOT NULL,
    POVKey      NVARCHAR(400) NOT NULL,                  -- Entity|Scenario|Time
    RowHash     BINARY(32)    NOT NULL,                  -- CHECKSUM/HASHBYTES of the source slice
    LastLoaded  DATETIME      NOT NULL,
    CONSTRAINT PK_FMM_CubeLoadWatermark PRIMARY KEY (LoadID, POVKey)
);
CREATE INDEX IX_FMM_CubeLoadWatermark_POV ON FMM_CubeLoadWatermark (LoadID, POVKey) INCLUDE (RowHash);

/* ---------------------------------------------------------------------------
   2) CUBE -> TABLE  (the construct RMW does NOT have; FDX -> SqlBulkCopy)
   --------------------------------------------------------------------------- */
CREATE TABLE FMM_CubeExtractConfig (
    ExtractID       INT           NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    SourceCubeView  NVARCHAR(200) NOT NULL,              -- the FDX-enabled cube view
    WorkspaceName   NVARCHAR(100) NOT NULL,
    TargetTable     NVARCHAR(200) NOT NULL,
    TimeFilterExpr  NVARCHAR(200) NULL,                  -- e.g. T#|!Time!|
    ParamMapJson    NVARCHAR(MAX) NULL,                  -- FDX NameValuePairs as JSON
    LoadMode        TINYINT       NOT NULL DEFAULT 1,    -- 1=TruncateReplace, 2=Delta(by key)
    Status          INT           NOT NULL DEFAULT 1,
    CreateDate DATETIME NULL, CreateUser NVARCHAR(100) NULL,
    UpdateDate DATETIME NULL, UpdateUser NVARCHAR(100) NULL,
    CONSTRAINT PK_FMM_CubeExtractConfig PRIMARY KEY (ExtractID)
);

-- FDX column (RowHdr#_Dim / ColVal#) -> destination table column.
CREATE TABLE FMM_CubeExtractColMap (
    ExtractID    INT           NOT NULL,
    FdxColumn    NVARCHAR(128) NOT NULL,                 -- e.g. RowHdr0_Entity, ColVal5_Amount
    TargetColumn NVARCHAR(128) NOT NULL,
    DataType     NVARCHAR(30)  NOT NULL DEFAULT 'nvarchar',
    IsKey        BIT           NOT NULL DEFAULT 0,       -- part of the delta/merge key
    SortOrder    INT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_FMM_CubeExtractColMap PRIMARY KEY (ExtractID, FdxColumn)
);

/* ---------------------------------------------------------------------------
   3) APPROVAL / WORKFLOW  (replaces the hardcoded VB Status_manager dictionaries)
   --------------------------------------------------------------------------- */
CREATE TABLE FMM_WorkflowStates (
    ProcessKey   NVARCHAR(50)  NOT NULL,                 -- e.g. PGM, SPLN, UFR (or a generic process)
    StateCode    NVARCHAR(60)  NOT NULL,                 -- e.g. L2_Formulate
    DisplayName  NVARCHAR(100) NOT NULL,
    TierLevel    INT           NULL,                     -- was parsed from Entity Text3 'EntityLevel##'
    IsTerminal   BIT           NOT NULL DEFAULT 0,
    SortOrder    INT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_FMM_WorkflowStates PRIMARY KEY (ProcessKey, StateCode)
);

CREATE TABLE FMM_WorkflowTransitions (
    ProcessKey    NVARCHAR(50) NOT NULL,
    FromState     NVARCHAR(60) NOT NULL,
    Action        NVARCHAR(40) NOT NULL,                 -- Submit|Approve|Validate|Prioritize|Demote
    ToState       NVARCHAR(60) NOT NULL,
    RequiredTier  INT          NULL,                     -- min tier allowed to perform it
    RequireComment BIT         NOT NULL DEFAULT 0,       -- e.g. demotions require a comment
    GuardExpr     NVARCHAR(400) NULL,                    -- optional predicate (cube annotation / config)
    SortOrder     INT          NOT NULL DEFAULT 0,
    CONSTRAINT PK_FMM_WorkflowTransitions PRIMARY KEY (ProcessKey, FromState, Action)
);

-- The state store (instance state), kept separate from history.
CREATE TABLE FMM_WorkflowState (
    ProcessKey    NVARCHAR(50)  NOT NULL,
    RecordID      NVARCHAR(60)  NOT NULL,                -- the REQ / row id
    CurrentState  NVARCHAR(60)  NOT NULL,
    CurrentEntity NVARCHAR(100) NULL,
    UpdateUser    NVARCHAR(100) NULL,
    UpdateDate    DATETIME      NULL,
    CONSTRAINT PK_FMM_WorkflowState PRIMARY KEY (ProcessKey, RecordID)
);
CREATE INDEX IX_FMM_WorkflowState_Scan ON FMM_WorkflowState (ProcessKey, CurrentState, CurrentEntity);

-- Batched audit (one set-based insert per action, NOT per-row-per-column like XFC_CMD_Audit).
CREATE TABLE FMM_AuditLog (
    AuditID     BIGINT IDENTITY(1,1) NOT NULL,
    ProcessKey  NVARCHAR(50)  NOT NULL,
    RecordID    NVARCHAR(60)  NOT NULL,
    ColumnName  NVARCHAR(128) NULL,
    OldValue    NVARCHAR(400) NULL,
    NewValue    NVARCHAR(400) NULL,
    ChangedBy   NVARCHAR(100) NULL,
    ChangedAt   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_FMM_AuditLog PRIMARY KEY (AuditID)
);
CREATE INDEX IX_FMM_AuditLog_Record ON FMM_AuditLog (ProcessKey, RecordID, ChangedAt);

/* ---------------------------------------------------------------------------
   4) DATA VALIDATION  (generalize the OSDAI condition-engine pattern)
   --------------------------------------------------------------------------- */
CREATE TABLE FMM_ValidationRules (
    RuleID     INT           NOT NULL,
    ScopeKey   NVARCHAR(50)  NOT NULL,                   -- which process/area the rule applies to
    Name       NVARCHAR(100) NOT NULL,
    RuleType   NVARCHAR(30)  NOT NULL,                   -- SQL | CubeView | Expression
    Expression NVARCHAR(MAX) NOT NULL,                   -- the check (SQL predicate / CV name / formula)
    Operator   NVARCHAR(10)  NULL,                       -- >, <, =, <> for threshold checks
    Threshold  NVARCHAR(50)  NULL,
    Severity   NVARCHAR(20)  NOT NULL DEFAULT 'Warning', -- Critical | Warning | Informational
    Message    NVARCHAR(400) NULL,
    IsActive   BIT           NOT NULL DEFAULT 1,
    SortOrder  INT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_FMM_ValidationRules PRIMARY KEY (RuleID)
);

CREATE TABLE FMM_ValidationResults (
    RunID       BIGINT        NOT NULL,
    RuleID      INT           NOT NULL,
    POVKey      NVARCHAR(400) NULL,
    Passed      BIT           NOT NULL,
    ActualValue NVARCHAR(100) NULL,
    Message     NVARCHAR(400) NULL,
    RunDate     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_FMM_ValidationResults PRIMARY KEY (RunID, RuleID, POVKey)
);
CREATE INDEX IX_FMM_ValidationResults_Fail ON FMM_ValidationResults (RunID, Passed) INCLUDE (Severity) ;

/* ---------------------------------------------------------------------------
   5) ASSUMPTIONS / DRIVERS  (extend the existing generic K/V store idea)
   XFC_CMD_Cycle_Param_Values already proves this shape works app-wide:
       (Command, Cycle, Column_Name, Value, InUse)
   Reuse it rather than inventing a new table; the engines read drivers/targets
   from here so escalation rates, target origins, flow codes etc. are data, not code.
   --------------------------------------------------------------------------- */
