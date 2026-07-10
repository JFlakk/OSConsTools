using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class MDM_ConfigHelpers
    {
        #region "Enums"

        public enum SaveType { Add, Update, View, Delete }

        /// <summary>The six feature areas of the MDM product.</summary>
        public enum FeatureType
        {
            DimMaintenance  = 1,
            Integration     = 2,
            Approval        = 3,
            Validation      = 4,
            Admin           = 5,
            Reports         = 6
        }

        /// <summary>Type of member change being requested.</summary>
        public enum ChangeType { Add = 1, Edit = 2, Move = 3, Retire = 4 }

        /// <summary>Lifecycle state of a change request.</summary>
        public enum ApprovalStatus
        {
            Draft       = 1,
            Submitted   = 2,
            InReview    = 3,
            Approved    = 4,
            Rejected    = 5,
            Applied     = 6,
            Withdrawn   = 7
        }

        /// <summary>Direction of an integration job.</summary>
        public enum IntegrationDirection { Upstream = 1, Downstream = 2, Bidirectional = 3 }

        /// <summary>Source/target connection type for an integration.</summary>
        public enum IntegrationSourceType { SQL = 1, FlatFile = 2, API = 3 }

        /// <summary>Validation rule categories.</summary>
        public enum RuleType
        {
            Uniqueness          = 1,
            NamingConvention    = 2,
            RequiredAttribute   = 3,
            HierarchyConstraint = 4,
            CrossDimConsistency = 5,
            CustomSQL           = 6
        }

        /// <summary>Severity of a validation violation.</summary>
        public enum RuleSeverity { Error = 1, Warning = 2, Info = 3 }

        /// <summary>Layout type for user-side content panes.</summary>
        public enum LayoutType
        {
            None                 = 0,
            Dashboard            = 1,
            Dashboard_CustomDB   = 2,
            CubeView             = 3,
            Dashboard_TopBottom  = 4,
            Dashboard_LeftRight  = 5,
            Dashboard_2Top1Bottom = 6,
            Dashboard_1Top2Bottom = 7,
            Dashboard_2Left1Right = 8,
            Dashboard_1Left2Right = 9,
            Dashboard_2x2        = 10
        }

        /// <summary>Content type for a resolved pane binding.</summary>
        public enum DBPaneContents { Dashboard = 1, CubeView = 2 }

        /// <summary>Active / Inactive record status.</summary>
        public enum RecordStatus { Active = 1, Inactive = 0 }

        #endregion

        #region "Config Setup — IConfigMappings"

        public interface IConfigMappings
        {
            Dictionary<int, Dictionary<string, string>> ParameterMappings { get; }
        }

        #endregion

        #region "Dim Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_DimConfig</c> column names for Add / Update / View.
        /// </summary>
        public class DimConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class DimConfigRegistry
        {
            public static readonly Dictionary<SaveType, DimConfig> Configs = new()
            {
                [SaveType.Add] = new DimConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_DimConfig_DimName",    "DimName"    } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_DimConfig_Descr",      "Descr"      } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_DimConfig_FeatureType","FeatureType" } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_DimConfig_Status",     "Status"     } } }
                    }
                },
                [SaveType.Update] = new DimConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_DimConfig_Descr",      "Descr"      } } },
                        { 1, new Dictionary<string, string> { { "DL_MDM_DimConfig_FeatureType","FeatureType" } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_DimConfig_Status",     "Status"     } } }
                    }
                },
                [SaveType.View] = new DimConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_DimConfig_DimName",    "DimName"    } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_DimConfig_Descr",      "Descr"      } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_DimConfig_FeatureType","FeatureType" } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_DimConfig_CreateDate", "CreateDate" } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_DimConfig_CreateUser", "CreateUser" } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_DimConfig_UpdateDate", "UpdateDate" } } },
                        { 6, new Dictionary<string, string> { { "IV_MDM_DimConfig_UpdateUser", "UpdateUser" } } }
                    }
                }
            };
        }

        #endregion

        #region "Integration Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_IntegrationConfig</c> column names.
        /// </summary>
        public class IntegrationConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class IntegrationConfigRegistry
        {
            public static readonly Dictionary<SaveType, IntegrationConfig> Configs = new()
            {
                [SaveType.Add] = new IntegrationConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_IntConfig_Name",       "Name"       } } },
                        { 1, new Dictionary<string, string> { { "BL_MDM_IntConfig_DimConfigID","DimConfigID"} } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_IntConfig_Direction",  "Direction"  } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_IntConfig_SourceType", "SourceType" } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_IntConfig_ConnString",  "ConnString" } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_IntConfig_Descr",       "Descr"      } } },
                        { 6, new Dictionary<string, string> { { "DL_MDM_IntConfig_Status",      "Status"     } } }
                    }
                },
                [SaveType.Update] = new IntegrationConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "DL_MDM_IntConfig_Direction",  "Direction"  } } },
                        { 1, new Dictionary<string, string> { { "DL_MDM_IntConfig_SourceType", "SourceType" } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_IntConfig_ConnString",  "ConnString" } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_IntConfig_Descr",       "Descr"      } } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_IntConfig_Status",      "Status"     } } }
                    }
                },
                [SaveType.View] = new IntegrationConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_IntConfig_Name",       "Name"       } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_IntConfig_DimConfigID","DimConfigID"} } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_IntConfig_Direction",  "Direction"  } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_IntConfig_SourceType", "SourceType" } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_IntConfig_Descr",       "Descr"      } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_IntConfig_CreateDate", "CreateDate" } } },
                        { 6, new Dictionary<string, string> { { "IV_MDM_IntConfig_CreateUser", "CreateUser" } } },
                        { 7, new Dictionary<string, string> { { "IV_MDM_IntConfig_UpdateDate", "UpdateDate" } } },
                        { 8, new Dictionary<string, string> { { "IV_MDM_IntConfig_UpdateUser", "UpdateUser" } } }
                    }
                }
            };
        }

        #endregion

        #region "Approval Workflow Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_ApprovalWorkflow</c> column names.
        /// </summary>
        public class ApprovalWorkflowConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class ApprovalWorkflowRegistry
        {
            public static readonly Dictionary<SaveType, ApprovalWorkflowConfig> Configs = new()
            {
                [SaveType.Add] = new ApprovalWorkflowConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_ApprWF_Name",       "Name"       } } },
                        { 1, new Dictionary<string, string> { { "BL_MDM_ApprWF_DimConfigID","DimConfigID"} } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_ApprWF_ChangeType", "ChangeType" } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ApprWF_Descr",      "Descr"      } } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_ApprWF_Status",     "Status"     } } }
                    }
                },
                [SaveType.Update] = new ApprovalWorkflowConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "DL_MDM_ApprWF_ChangeType", "ChangeType" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ApprWF_Descr",      "Descr"      } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_ApprWF_Status",     "Status"     } } }
                    }
                },
                [SaveType.View] = new ApprovalWorkflowConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_ApprWF_Name",       "Name"       } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ApprWF_DimConfigID","DimConfigID"} } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_ApprWF_ChangeType", "ChangeType" } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ApprWF_Descr",      "Descr"      } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_ApprWF_CreateDate", "CreateDate" } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_ApprWF_CreateUser", "CreateUser" } } },
                        { 6, new Dictionary<string, string> { { "IV_MDM_ApprWF_UpdateDate", "UpdateDate" } } },
                        { 7, new Dictionary<string, string> { { "IV_MDM_ApprWF_UpdateUser", "UpdateUser" } } }
                    }
                }
            };
        }

        #endregion

        #region "Approval Step Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_ApprovalStep</c> column names.
        /// </summary>
        public class ApprovalStepConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class ApprovalStepRegistry
        {
            public static readonly Dictionary<SaveType, ApprovalStepConfig> Configs = new()
            {
                [SaveType.Add] = new ApprovalStepConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_ApprStep_WorkflowID", "WorkflowID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ApprStep_StepOrder",  "StepOrder"  } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_ApprStep_Name",       "Name"       } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ApprStep_Assignee",   "Assignee"   } } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_ApprStep_Status",     "Status"     } } }
                    }
                },
                [SaveType.Update] = new ApprovalStepConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_ApprStep_StepOrder",  "StepOrder"  } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ApprStep_Name",       "Name"       } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_ApprStep_Assignee",   "Assignee"   } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_ApprStep_Status",     "Status"     } } }
                    }
                },
                [SaveType.View] = new ApprovalStepConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_ApprStep_WorkflowID", "WorkflowID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ApprStep_StepOrder",  "StepOrder"  } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_ApprStep_Name",       "Name"       } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ApprStep_Assignee",   "Assignee"   } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_ApprStep_CreateDate", "CreateDate" } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_ApprStep_CreateUser", "CreateUser" } } }
                    }
                }
            };
        }

        #endregion

        #region "Validation Rule Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_ValidationRule</c> column names.
        /// Stores flexible rule settings in <c>ConfigJSON</c> mirroring the FMM DataValConfig pattern
        /// (keys: ruleType, targetDim, expression, severity).
        /// </summary>
        public class ValidationRuleConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class ValidationRuleRegistry
        {
            public static readonly Dictionary<SaveType, ValidationRuleConfig> Configs = new()
            {
                [SaveType.Add] = new ValidationRuleConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_ValRule_DimConfigID", "DimConfigID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ValRule_Name",        "Name"        } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_ValRule_RuleType",    "RuleType"    } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_ValRule_Severity",    "Severity"    } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_ValRule_ConfigJSON",  "ConfigJSON"  } } },
                        { 5, new Dictionary<string, string> { { "DL_MDM_ValRule_Status",      "Status"      } } }
                    }
                },
                [SaveType.Update] = new ValidationRuleConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_ValRule_Name",       "Name"        } } },
                        { 1, new Dictionary<string, string> { { "DL_MDM_ValRule_RuleType",   "RuleType"    } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_ValRule_Severity",   "Severity"    } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ValRule_ConfigJSON", "ConfigJSON"  } } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_ValRule_Status",     "Status"      } } }
                    }
                },
                [SaveType.View] = new ValidationRuleConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_ValRule_DimConfigID","DimConfigID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_ValRule_Name",       "Name"        } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_ValRule_RuleType",   "RuleType"    } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_ValRule_Severity",   "Severity"    } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_ValRule_ConfigJSON", "ConfigJSON"  } } },
                        { 5, new Dictionary<string, string> { { "IV_MDM_ValRule_CreateDate", "CreateDate"  } } },
                        { 6, new Dictionary<string, string> { { "IV_MDM_ValRule_CreateUser", "CreateUser"  } } },
                        { 7, new Dictionary<string, string> { { "IV_MDM_ValRule_UpdateDate", "UpdateDate"  } } },
                        { 8, new Dictionary<string, string> { { "IV_MDM_ValRule_UpdateUser", "UpdateUser"  } } }
                    }
                }
            };
        }

        #endregion

        #region "Access Config"

        /// <summary>
        /// Maps UI component names to <c>MDM_AccessConfig</c> column names.
        /// </summary>
        public class AccessConfig : IConfigMappings
        {
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class AccessConfigRegistry
        {
            public static readonly Dictionary<SaveType, AccessConfig> Configs = new()
            {
                [SaveType.Add] = new AccessConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "BL_MDM_Access_DimConfigID", "DimConfigID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_Access_GroupName",   "GroupName"   } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_Access_Role",        "Role"        } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_Access_Status",      "Status"      } } }
                    }
                },
                [SaveType.Update] = new AccessConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "DL_MDM_Access_Role",   "Role"   } } },
                        { 1, new Dictionary<string, string> { { "DL_MDM_Access_Status", "Status" } } }
                    }
                },
                [SaveType.View] = new AccessConfig
                {
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_Access_DimConfigID", "DimConfigID" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_Access_GroupName",   "GroupName"   } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_Access_Role",        "Role"        } } },
                        { 3, new Dictionary<string, string> { { "IV_MDM_Access_CreateDate",  "CreateDate"  } } },
                        { 4, new Dictionary<string, string> { { "IV_MDM_Access_CreateUser",  "CreateUser"  } } }
                    }
                }
            };
        }

        #endregion

        #region "Menu Layout Config"

        /// <summary>
        /// Admin-side menu layout config. Maps setup-options selection to a content dashboard.
        /// </summary>
        public class MenuLayoutConfig : IConfigMappings
        {
            public string Config_DashboardName { get; init; }
            public string DashboardName        { get; init; }
            public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
        }

        public static class MenuLayoutRegistry
        {
            public static readonly Dictionary<LayoutType, MenuLayoutConfig> Configs = new()
            {
                [LayoutType.Dashboard] = new MenuLayoutConfig
                {
                    Config_DashboardName = "MDM_LayoutConfig_DB",
                    DashboardName        = "MDM_App_Content_DB",
                    ParameterMappings    = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_MenuLayout_SortOrder", "SortOrder" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_MenuLayout_Name",      "Name"      } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_MenuLayout_DB_Name",   "DB_Name"   } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_MenuLayout_Status",    "Status"    } } }
                    }
                },
                [LayoutType.CubeView] = new MenuLayoutConfig
                {
                    DashboardName     = "MDM_LayoutConfig_CV",
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_MenuLayout_SortOrder", "SortOrder" } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_MenuLayout_Name",      "Name"      } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_MenuLayout_CV_Name",   "CV_Name"   } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_MenuLayout_Status",    "Status"    } } }
                    }
                },
                [LayoutType.Dashboard_TopBottom] = new MenuLayoutConfig
                {
                    DashboardName     = "MDM_LayoutConfig_TB_DB",
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_MenuLayout_SortOrder",   "SortOrder"   } } },
                        { 1, new Dictionary<string, string> { { "IV_MDM_MenuLayout_Name",        "Name"        } } },
                        { 2, new Dictionary<string, string> { { "IV_MDM_MenuLayout_T_Height",    "T_Height"    } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_MenuLayout_T_ContentType","T_ContentType"} } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_MenuLayout_T_Name",      "T_Name"      } } },
                        { 5, new Dictionary<string, string> { { "DL_MDM_MenuLayout_B_ContentType","B_ContentType"} } },
                        { 6, new Dictionary<string, string> { { "DL_MDM_MenuLayout_B_Name",      "B_Name"      } } },
                        { 7, new Dictionary<string, string> { { "DL_MDM_MenuLayout_Status",      "Status"      } } }
                    }
                },
                [LayoutType.Dashboard_LeftRight] = new MenuLayoutConfig
                {
                    DashboardName     = "MDM_LayoutConfig_LR_DB",
                    ParameterMappings = new()
                    {
                        { 0, new Dictionary<string, string> { { "IV_MDM_MenuLayout_L_Width",       "L_Width"       } } },
                        { 1, new Dictionary<string, string> { { "DL_MDM_MenuLayout_L_ContentType", "L_ContentType" } } },
                        { 2, new Dictionary<string, string> { { "DL_MDM_MenuLayout_L_Name",        "L_Name"        } } },
                        { 3, new Dictionary<string, string> { { "DL_MDM_MenuLayout_R_ContentType", "R_ContentType" } } },
                        { 4, new Dictionary<string, string> { { "DL_MDM_MenuLayout_R_Name",        "R_Name"        } } }
                    }
                }
            };
        }

        #endregion
    }
}
