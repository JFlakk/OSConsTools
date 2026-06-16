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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.FMM_DataSets
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        private string defaultStringVal = "0"; // first item in the list
        #endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, $"Hit {args.DataSetName}");
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSet:

                        if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_OSCubes"))
                        {
                            return get_FMM_OSCubes();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ScenTypes"))
                        {
                            return get_FMM_ScenTypes();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CubeConfigs"))
                        {
                            return get_FMM_Cubes("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cubes_TableAct"))
                        {
                            return get_FMM_Cubes("TableAct");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cubes_Src"))
                        {
                            return get_FMM_Cubes("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cubes_Tgt"))
                        {
                            return get_FMM_Cubes("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ActConfig_Src"))
                        {
                            return Get_FMM_ActConfig("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_ActConfig_Tgt"))
                        {
                            return Get_FMM_ActConfig("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_ActConfig_Table"))
                        {
                            return Get_FMM_ActConfig("Table");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_ActConfig"))
                        {
                            return Get_FMM_ActConfig("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelConfig_All"))
                        {
                            return get_FMM_ModelConfigs("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelConfigs_Act"))
                        {
                            return get_FMM_ModelConfigs("Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelConfig_Src"))
                        {
                            return get_FMM_ModelConfigs("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelConfig_Tgt"))
                        {
                            return get_FMM_ModelConfigs("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Calcs_Model"))
                        {
                            return get_FMM_Calcs("Model");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CalcConfigs"))
                        {
                            return get_FMM_CalcConfigs("By_Model");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelGrpConfigs_All"))
                        {
                            return get_FMM_ModelGrpConfigs("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelGrpConfigs_Cube"))
                        {
                            return get_FMM_ModelGrpConfigs("Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelGrpConfigseqs_Cube"))
                        {
                            return get_FMM_ModelGrpConfigseqs("Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_WFChannels"))
                        {
                            return get_FMM_WFChannels();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CalcUnits"))
                        {
                            return get_FMM_CalcUnits("By_Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Calc_Units"))
                        {
                            return get_FMM_CalcUnits("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_UIConfigs_By_Act"))
                        {
                            return get_FMM_UIConfigs("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_RegConfigs"))
                        {
                            return get_FMM_UIConfigs("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_RegConfigs"))
                        {
                            return get_FMM_UIConfigs("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CustTables"))
                        {
                            return get_FMM_CustTables("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CustTables_Audit"))
                        {
                            return get_FMM_CustTables("Audit");
                        }
                        //Return WF Profile Hierarchy for WF Root Profile selected
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_WFProfile_TreeView"))
                        {
                            return get_FMM_WFProfile_Hierarchy();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ApprConfig_Config"))
                        {
                            return get_FMM_ApprConfigs("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Security_Grps"))
                        {
                            return get_FMM_Security_Grps();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_WFP"))
                        {
                            return get_FMM_WFProfiles("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_UnitConfigs"))
                        {
                            return get_FMM_UnitConfigs();
                        }


                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Helper Queries"
        private DataTable get_FMM_OSCubes()
        {
            try
            {
                var dt = new DataTable("FMM_OSCubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT CubeID as Value, Name
                                        FROM Cube
                                        WHERE IsTopLevelCube = 1";
                    var sqlparams = new SqlParameter[]
                    {};
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ScenTypes()
        {
            try
            {
                var cube = args.NameValuePairs.XFGetValue("Cube", "0").XFConvertToInt();
                var dt = new DataTable("ScenType");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sql = @"WITH ScenType_List (ScenType) AS (
                                    SELECT 'Actual' UNION ALL SELECT 'Administration' UNION ALL SELECT 'Budget' UNION ALL
                                    SELECT 'Control' UNION ALL SELECT 'Flash' UNION ALL SELECT 'Forecast' UNION ALL
                                    SELECT 'FXModel' UNION ALL SELECT 'History' UNION ALL SELECT 'LongTerm' UNION ALL
                                    SELECT 'Model' UNION ALL SELECT 'Operational' UNION ALL SELECT 'Plan' UNION ALL
                                    SELECT 'Sustainability' UNION ALL SELECT 'Target' UNION ALL SELECT 'Tax' UNION ALL
                                    SELECT 'Variance' UNION ALL SELECT 'ScenarioType1' UNION ALL SELECT 'ScenarioType2' UNION ALL
                                    SELECT 'ScenarioType3' UNION ALL SELECT 'ScenarioType4' UNION ALL SELECT 'ScenarioType5' UNION ALL
                                    SELECT 'ScenarioType6' UNION ALL SELECT 'ScenarioType7' UNION ALL SELECT 'ScenarioType8'
                                )
                                SELECT ScenType, ScenType as ScenTypeID FROM ScenType_List
                                WHERE NOT EXISTS (
                                    SELECT 1 FROM FMM_CubeConfig
                                    WHERE ScenType = ScenType_List.ScenType AND Cube = @Cube)";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.VarChar) { Value = cube }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Cubes(string cubeType)
        {
            try
            {
                var sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube, CubeConfigID
                            FROM FMM_CubeConfig";
                var tableName = "CubeConfig";

                switch (cubeType.ToUpperInvariant())
                {
                    case "SRC":
                        tableName = "CubeConfig_Src";
                        break;
                    case "TGT":
                        tableName = "CubeConfig_Tgt";
                        break;
                    case "TABLEACT":
                        tableName = "CubeConfig_TableAct";
                        sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube, Cube.CubeConfigID
                            FROM FMM_CubeConfig Cube
                            JOIN FMM_ActConfig Act ON Cube.CubeConfigID = Act.CubeConfigID";
                        break;
                }

                var dt = new DataTable(tableName);
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, new SqlParameter[] { });
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_ActConfig(string actType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                var dt = new DataTable("FMM_ActConfig");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
					
                    var sql = @"SELECT Name, ActConfigID as Value
                            FROM FMM_CubeConfig Con
                            JOIN FMM_ActConfig Act ON Con.CubeConfigID = Act.CubeConfigID
                            WHERE Con.CubeConfigID = @CubeConfigID
                            ORDER BY Cube, ScenType, Name";

                    var sqlparams = new[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID.XFConvertToInt() } };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_UnitConfigs()
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "-1");
                var dt = new DataTable("UnitConfigs");

                var sql = @"SELECT Name, UnitID
	                        FROM FMM_UnitConfig
	                        WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID
	                        ORDER BY Name";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sqlparams = new[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID.XFConvertToInt() },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID.XFConvertToInt() }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_CustTables(string custTableType)
        {
            try
            {
                var wsID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Consultant Tools");
                var paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, args.NameValuePairs, wsID, "DL_FMM_CustTableConfig_Type");

                var cases = string.Empty;
                if (paramInfo.ComboBoxItemsForBoundList.Count > 0)
                {
                    cases = string.Join(" ", paramInfo.ComboBoxItemsForBoundList.Select(item => $"WHEN {item.Value} THEN '{item.Name}'"));
                }
                var dt = new DataTable("CustTable");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = string.Empty;
                    var sqlparams = new SqlParameter[]
                        {
                        };

                    // Determine SQL and parameters based on custTableType
                    switch (custTableType.ToUpperInvariant())
                    {
                        case "ALL":
                            dt.TableName = "FMM_CustTableConfig";
                            sql = $@"SELECT CONCAT(Name, ' - ',
                                        CASE Type 
                                            {cases} 
                                        ELSE CAST(Type AS VARCHAR) 
                                        END) AS Name,CustTableConfigID as Value
                                        FROM FMM_CustTableConfig
                                        ORDER BY Type,Name";
                            sqlparams = new SqlParameter[]
                            {
                            };
                            break;
                    }

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ApprConfigoval_Acts()
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1").XFConvertToInt();
                var ApprID = args.NameValuePairs.XFGetValue("ApprID", "-1").XFConvertToInt();
                var appr_Step_ID = args.NameValuePairs.XFGetValue("Appr_Step_ID", "-1").XFConvertToInt();
                var addUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName", string.Empty);
                var isUpdate = addUpdateDBName.XFEqualsIgnoreCase("0b3b2a2_FMM_ApprConfigoval_Steps_Activities_Row2b_Header");

                var dt = new DataTable("Approval_Act_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    string sql = isUpdate
                        ? @"SELECT CONCAT(Con.Name, ' - ', Con.CalcType) AS Activity, Approval_Step_ActConfigID AS ActConfigID
                            FROM FMM_ApprConfigoval_Step_Activity_Config ASAC
                            FULL JOIN FMM_ActConfig Con ON Con.ActConfigID = ASAC.ActConfigID
                            WHERE ASAC.ApprID = @ApprID AND ASAC.Appr_Step_ID = @Appr_Step_ID
                            ORDER BY Approval_Step_ActConfigID"
                        : @"Declare @CubeActivityResults Table (Activity nvarchar(100), ActConfigID int)
                            INSERT INTO @CubeActivityResults
                            SELECT CONCAT(Name, ' - ', CalcType) AS Activity, ActConfigID
                            FROM FMM_CubeConfig Con
                            JOIN FMM_ActConfig Act ON Con.CubeConfigID = Act.CubeConfigID
                            WHERE Con.CubeConfigID = @CubeConfigID AND Act.CalcType IN (1, 2)
                            ORDER BY Cube, ScenType, Name
                            SELECT CAR.Activity, CAR.ActConfigID FROM @CubeActivityResults CAR
                            FULL JOIN FMM_ApprConfigoval_Step_Activity_Config ASAC ON CAR.ActConfigID = ASAC.ActConfigID
                            WHERE ASAC.Approval_Step_ActConfigID IS NULL";

                    var sqlparams = isUpdate
                        ? new SqlParameter[]
                        {
                            new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID },
                            new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = appr_Step_ID }
                        }
                        : new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID }
                        };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ModelConfigs(string modelType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "0");
                var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "0");
                var dt = new DataTable("FMM_ModelConfig");

                string tableName;
                string sql;
                switch (modelType.ToUpperInvariant())
                {
                    case "SRC":
                        tableName = "Models_Src";
                        sql = @"SELECT Name, ModelConfigID as Value
                        FROM FMM_ModelConfig
                        WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID
                        ORDER BY Name";
                        break;
                    case "TGT":
                        tableName = "Models_Tgt";
                        sql = @"SELECT Name, ModelConfigID as Value
                        FROM FMM_ModelConfig
                        WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID
                        ORDER BY Name";
                        break;
                    case "ACT":
                        tableName = "FMM_ModelConfig";
                        sql = @"SELECT Name, ModelConfigID as Value
                        FROM FMM_ModelConfig
                        WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID
                        ORDER BY Name";
                        break;
                    default:
                        tableName = "FMM_ModelConfig";
                        sql = @"SELECT Name, ModelConfigID as Value
                        FROM FMM_ModelConfig
                        ORDER BY Name";
                        break;
                }
                dt.TableName = tableName;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sqlparams = new[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID.XFConvertToInt() },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID.XFConvertToInt() }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private DataTable get_FMM_Calcs(string calcType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "0").XFConvertToInt();
                var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "0").XFConvertToInt();
                var ModelConfigID = args.NameValuePairs.XFGetValue("ModelConfigID", "0").XFConvertToInt();

                var dt = new DataTable("Calc_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sql = @"SELECT Name, CalcConfigID
                                FROM FMM_CalcConfig
                                WHERE CubeConfigID = @CubeConfigID
                                AND ActConfigID = @ActConfigID
                                AND ModelConfigID = @ModelConfigID
                                ORDER BY Name";

                    var sqlparams = new[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID },
                        new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = ModelConfigID }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_CalcConfigs(string calcType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "0").XFConvertToInt();
                var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "0").XFConvertToInt();
                var ModelConfigID = args.NameValuePairs.XFGetValue("ModelConfigID", "0").XFConvertToInt();

                var dt = new DataTable("FMM_CalcConfigs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT CONCAT(Sequence,' - ',Name) as Name, CalcConfigID
                                FROM FMM_CalcConfig
                                WHERE CubeConfigID = @CubeConfigID
                                AND ActConfigID = @ActConfigID
                                AND ModelConfigID = @ModelConfigID
                                ORDER BY Name";

                    var sqlparams = new[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID },
                        new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = ModelConfigID }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ModelGrpConfigs(string modelGrpType)
        {
            try
            {
                var dt = new DataTable("ModelGrps");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sql = @"SELECT Name, ModelGrpID FROM FMM_ModelGrps ORDER BY Name";

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, System.Array.Empty<SqlParameter>());
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ModelGrpConfigseqs(string modelGrpSeqType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1").XFConvertToInt();
                var dt = new DataTable("ModelGrpSeqs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sql = @"SELECT Name, ModelGrpSeqID
                                FROM FMM_ModelGrpSeqs
                                WHERE CubeConfigID = @CubeConfigID
                                ORDER BY Name";

                    var sqlparams = new[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID } };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_UIConfigs(string registerType)
        {
            try
            {
                var dt = new DataTable("Register_Configs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                var sql = string.Empty;
                var sqlparams = new SqlParameter[] { };

                switch (registerType.ToUpperInvariant())
                {
                    case "BY_ACT":
                        {
                            var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                            var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "-1");
                            dt.TableName = "RegConfigs_By_Act";
                            sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                            FROM FMM_UIConfig
                            WHERE CubeConfigID = @CubeConfigID
                            AND ActConfigID = @ActConfigID
                            ORDER BY Name";
                            sqlparams = new SqlParameter[]
                            {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = Convert.ToInt16(CubeConfigID) },
                            new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = Convert.ToInt16(ActConfigID) }
                            };
                            break;
                        }
                    case "SOURCE":
                        {
                            var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                            dt.TableName = "RegConfigs_Source";
                            sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                            FROM FMM_UIConfig
                            WHERE CubeConfigID = @CubeConfigID
                            ORDER BY Name";
                            sqlparams = new SqlParameter[]
                            {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = Convert.ToInt16(CubeConfigID) }
                            };
                            break;
                        }
                    case "TARGET":
                        {
                            var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                            dt.TableName = "RegConfigs_Target";
                            sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                            FROM FMM_UIConfig
                            WHERE CubeConfigID = @CubeConfigID
                            ORDER BY Name";
                            sqlparams = new SqlParameter[]
                            {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = Convert.ToInt16(CubeConfigID) }
                            };
                            break;
                        }
                    default:
                        dt.TableName = "RegConfigs";
                        sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                            FROM FMM_UIConfig
                            ORDER BY Name";
                        sqlparams = new SqlParameter[] { };
                        break;
                }

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_WFChannels()
        {
            try
            {
                var dt = new DataTable("FMM_WF_Channels");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"
                                SELECT Name as DisplayValue,Name as StoredValue
                                FROM WorkflowChannel
                                UNION
                                SELECT 'Standard' as DisplayValue,'Standard' as StoredValue
                                UNION
                                SELECT 'NoDataLock' as DisplayValue,'NoDataLock' as StoredValue
                                UNION
                                SELECT 'AllChannelInput' as DisplayValue,'AllChannelInput' as StoredValue";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_CalcUnits(string calcUnitType)
        {
            try
            {
                var dt = new DataTable("FMM_CalcUnitConfig");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = string.Empty;
                    SqlParameter[] sqlparams;

                    switch (calcUnitType.ToUpperInvariant())
                    {
                        case "BY_CUBE":
                            {
                                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1");
                                dt.TableName = "Calc_Units_By_Cube";
                                sql = @"
                                    SELECT CONCAT(EntityMFB, ' - ',WFChannel) as Calc_Unit_Desc,CalcUnitID
                                    FROM FMM_CalcUnitConfig
                                    WHERE CubeConfigID = @CubeConfigID";
                                sqlparams = new SqlParameter[]
                                {
                                new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = Convert.ToInt16(CubeConfigID) }
                                };
                                break;
                            }
                        default: // "All" or default
                            dt.TableName = "FMM_CalcUnitConfig";
                            sql = @"
                                    SELECT CONCAT(EntityMFB, ' - ',WFChannel) as Calc_Unit_Desc,CalcUnitID
                                    FROM FMM_CalcUnitConfig";
                            sqlparams = new SqlParameter[] { };
                            break;
                    }

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }


                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Fdx_Cube_View()
        {
            try
            {

                var objDashboardWorkspace = BRApi.Dashboards.Workspaces.GetWorkspace(si, false, "Gov_PlanCycle");
                var dt = new DataTable();
                var nvbParams = new NameValueFormatBuilder();
                nvbParams.NameValuePairs.XFSetValue("DL_PGM_Personnel_Type", "Dir");
                dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, objDashboardWorkspace.WorkspaceID, "01_Civ_Con_Pay_Rates_by_PE", "DHP_Consol_Entities_Dim", "E#HQ_ManpowerFactors", "Main_Scenario_Dim", "S#POM2630_v0", "", nvbParams, false, false, false, string.Empty, 8, true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataSet get_FMM_WFProfile_Hierarchy()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                var rootProfileName = "Army_RMW_Consol_CMD_PGM"; //args.CustomSubstVars.XFGetValue("BL_WFM_Root_WF_Profiles");

                var dt = new DataTable();

                // Define the SQL Statement
                var select_sql = @"
                        WITH RecursiveCTE AS (
                            SELECT 
                                prof.ProfileKey,
                                prof.ProfileName, 
                                CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentProfileKey, 
                                prof.HierarchyLevel, 
                                prof.HierarchyIndex
                            FROM 
                                WorkflowProfileHierarchy prof
                            WHERE 
                                prof.HierarchyLevel = 1
                                AND prof.IsTemplate = 0
                                AND prof.ProfileName = @rootprofilename 
                            UNION ALL
                            SELECT 
                                prof.ProfileKey,
                                prof.ProfileName,
                                prof.ParentProfileKey, 
                                prof.HierarchyLevel, 
                                prof.HierarchyIndex
                            FROM 
                                WorkflowProfileHierarchy prof
                            INNER JOIN 
                                RecursiveCTE rcte ON prof.ParentProfileKey = rcte.ProfileKey
                        )
                        SELECT 
                            rcte.ProfileName, 
                            rcte.ProfileKey,
                            rcte.ParentProfileKey, 
                            parentProf.ProfileName as ParentProfileName, 
                            rcte.HierarchyLevel,
                            rcte.HierarchyIndex
                        FROM 
                            RecursiveCTE rcte
                        LEFT JOIN 
                            WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
                        ORDER BY 
                            rcte.HierarchyLevel DESC, 
                            rcte.HierarchyIndex";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {
                                new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, select_sql, sqlparams);
                }

                foreach (DataRow row in dt.Rows)
                {
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string profileName = row["ProfileName"].ToString();
                    string profileKey = row["ProfileKey"].ToString();
                    string parentprofileName = row["ParentProfileName"].ToString();
                    string parentprofileKey = row["ParentProfileKey"].ToString();
                    parent_child.Add(profileName, parentprofileName);
                    var childProfiles = parent_child.Where(pair => pair.Value == profileName)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {

                            // Do something with each childProfile
                            // For example, create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == childProfile);

                            hierarchy_children_mbrs.Add(childXFTreeItem);
                        }
                        var wfprofile_xftreeitem = new XFTreeItem(profileName, profileName, string.Empty, false, true, false, false, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.FalseValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(profileName, profileName, string.Empty, false, true, false, false, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.FalseValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                }

                var hierXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == rootProfileName);
                hierarchy.TreeItems.Add(hierXFTreeItem);
                return hierarchy.CreateDataSet(si);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_ApprConfigs(string apprType)
        {
            try
            {
                var CubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "-1").XFConvertToInt();
                var dt = new DataTable("Approval_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = "SELECT * FROM FMM_ApprConfig_Config WHERE CubeConfigID = @CubeConfigID";
                    var sqlparams = new[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID } };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_UIConfig(string regConfigType)
        {

            try
            {
                var RegConfig_DT = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    // get all the register configs that don't already have an entry for this activity
                    var sql = @"
                                SELECT Name,RegCon.RegConfigID FROM FMM_UIConfig RegCon";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, RegConfig_DT, sql, sqlparams);

                }

                return RegConfig_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Reg_Profiles(string regProfilesType)
        {
            try
            {
                var ActConfigID = args.NameValuePairs.XFGetValue("ActConfigID", "-1");
                //var runType = args.CustomSubstVars.XFGetValue("IV_FMM_Display_Add_Update_Approval_Step_Activity") == "0b3b2a3_FMM_ApprConfigoval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var AddUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName");
                var runType = AddUpdateDBName == "0b3b2a2_FMM_ApprConfigoval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var dt = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (ActConfigID != "-1" && runType == "Add")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get all the register configs that don't already have an entry for this activity
                        var sql = @"
                                    SELECT Name,RegCon.RegConfigID FROM FMM_UIConfig RegCon
                                    FULL JOIN FMM_ApprConfigoval_Step_Activity_Config ASAC
                                    ON RegCon.ActConfigID = ASAC.ActConfigID AND RegCon.RegConfigID = ASAC.RegConfigID
                                    WHERE RegCon.ActConfigID = @ActConfigID AND ASAC.RegConfigID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                                    new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID.XFConvertToInt() },
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                    }
                }
                else if (ActConfigID != "-1" && runType == "Update")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get the current register config, and all of those that don't have associated entries (update)
                        var sql = @"
                                    Declare @initialResults Table (Name nvarchar(100), RegConfigID int, ActConfigID int)
                                    Declare @trueActConfigID int;
                                    
                                    Insert into @initialResults 
                                    Select Name,RegCon.RegConfigID,RegCon.ActConfigID FROM FMM_UIConfig RegCon
                                    Full JOIN FMM_ApprConfigoval_Step_Activity_Config ASAC
                                    ON RegCon.ActConfigID = ASAC.ActConfigID AND RegCon.RegConfigID = ASAC.RegConfigID
                                    WHERE ASAC.Approval_Step_ActConfigID = @ActConfigID

                                    SELECT @trueActConfigID = ActConfigID from @initialResults;
                                    
                                    Select CONCAT('*',Name) As Name, RegConfigID from @initialResults
                                    Union
                                    SELECT Name,RegCon.RegConfigID FROM FMM_UIConfig RegCon
                                    FULL JOIN FMM_ApprConfigoval_Step_Activity_Config ASAC
                                    ON RegCon.ActConfigID = ASAC.ActConfigID AND RegCon.RegConfigID = ASAC.RegConfigID
                                    WHERE RegCon.ActConfigID = @trueActConfigID AND ASAC.RegConfigID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                                    new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = ActConfigID.XFConvertToInt() }, // in the update case, ActConfigID is actually the approval_step_ActConfigID. It's weird
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Security_Grps()
        {
            try
            {
                var security_Group_DT = new DataTable("Security_Groups");
                security_Group_DT.Columns.Add("GroupName", typeof(string));
                security_Group_DT.Columns.Add("Description", typeof(string));

                var objList = BRApi.Security.Admin.GetGroups(si);
                foreach (var group in objList)
                {
                    security_Group_DT.Rows.Add(group.Name, group.Description ?? string.Empty);
                }

                return security_Group_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_WFProfiles(string wfProfileType)
        {
            try
            {
                var dt = new DataTable("WFProfiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"Select ProfileName,ProfileKey
                                                FROM WorkflowProfileHierarchy
                                                Where CubeName <> '-1'";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

    }
}