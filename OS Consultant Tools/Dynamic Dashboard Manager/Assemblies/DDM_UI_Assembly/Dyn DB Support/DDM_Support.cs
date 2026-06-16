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
using OneStreamWorkspacesApi.V820;
using Workspace.OSConsTools.DDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Support
    {
        //Params
        public const string Param_CubeName = "IV_DDM_App_CubeName";
        public const string Param_DashboardMenu = "BL_DDM_App_Menu";
		


        public object Test(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static string get_CubeName(SessionInfo si, int cubeId)
        {
            var cubeName = string.Empty;
            try
            {
                var dt = new DataTable("Cubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and parameters
                    var sql = @"SELECT Name
					       		FROM Cube
					       		WHERE CubeId = @OS_Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@OS_Cube_ID", SqlDbType.Int) { Value = cubeId }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }
                if (dt.Rows.Count > 0)
                {
                    cubeName = dt.Rows[0]["Name"].ToString();
                }

                return cubeName;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static Dictionary<string, string> get_ParamsToAdd(DataTable headerItems)
        {
            var paramVals = new Dictionary<string, string>();

            foreach (DataRow item in headerItems.Rows)
            {

                var baseSearch = string.Empty;

                // HdrType is an int in DDM_DynDBHdrConfig; resolve to its enum name
                var optTypeValue = Convert.ToInt32(item["HdrType"]);
                var optType = Enum.GetName(typeof(DDM_ConfigHelpers.HdrType), optTypeValue);

                if (optType == "Filter")
                {
                    baseSearch += "Fltr";

                    foreach (string colSuffix in DDM_Header.dashboardTypeResolver.Keys)
                    {
                        // Fltr_Btn / Fltr_Cbx / Fltr_Txt are bit columns; guard for null
                        string colName = baseSearch + "_" + colSuffix;
                        bool isEnabled = headerItems.Columns.Contains(colName)
                            && item[colName] != DBNull.Value
                            && Convert.ToBoolean(item[colName]);

                        if (isEnabled && colSuffix != "Txt")
                        {
                            // Fltr_DimType is an int in the schema; resolve to its enum name
                            var dimTypeValue = Convert.ToInt32(item[baseSearch + "_DimType"]);
                            string dimType = Enum.GetName(typeof(DDM_ConfigHelpers.HdrDimType), dimTypeValue);

                            // set the ML value here directly
                            if (paramVals.ContainsKey($"ML_DDM_App_{dimType}_Selection"))
                            {
                                paramVals[$"ML_DDM_App_{dimType}_Selection"] = item[baseSearch + "_Default"].ToString();
                            }
                            else
                            {
                                paramVals.Add($"ML_DDM_App_{dimType}_Selection", item[baseSearch + "_Default"].ToString());
                            }
                        }
                    }
                }
            }

            return paramVals;
        }

        public static int get_CurrProfileID(SessionInfo si, Guid profileKey)
        {
            var dt = new DataTable("configProfileDT");

            var profileID = -1;

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqa = new SqlDataAdapter();

                var sql = @"Select DynDBConfigID
                            From DDM_DynDBConfig
                            Where WFPKey = @OS_ProfileKey";

                var sqlparams = new SqlParameter[] {
                    new SqlParameter("@OS_ProfileKey", SqlDbType.UniqueIdentifier) { Value = profileKey }
                };

                if (profileKey != null)
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }

            if (dt.Rows.Count > 0)
            {
                profileID = Convert.ToInt32(dt.Rows[0]["DynDBConfigID"]);
            }

            return profileID;
        }

        public static DataTable get_ConfigMenu(SessionInfo si, int SelectedMenu)
        {

            var dt = new DataTable("ddm_dynDBMenuLayoutConfig_DT");
            if (SelectedMenu != -1)
            {
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                    var sqa = new SqlDataAdapter();

                    var sql = @"Select *
                                From DDM_DynDBMenuLayoutConfig
                                Where DynDBMenuID = @Menu_Option"; 

                    var sqlparams = new SqlParameter[] {
                        new SqlParameter("@Menu_Option", SqlDbType.Int) { Value = SelectedMenu }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }
// BRApi.ErrorLog.LogMessage(si, $" Get config menu {dt.Rows.Count}");
            return dt;

        }

        public static int get_SelectedMenu(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            var menuOption = -1;

            var menuOptionStr = customSubstVars.XFGetValue(Param_DashboardMenu, "1");


            if (!String.IsNullOrEmpty(menuOptionStr))
            {
                menuOption = Convert.ToInt32(menuOptionStr);
            }

            return menuOption;
        }

        public static DataTable get_HeaderItems(SessionInfo si, Dictionary<string, string> customSubstVarsAlreadyResolved, int option_Type)
        {
            var menu_option = customSubstVarsAlreadyResolved.XFGetValue(Param_DashboardMenu,"1");

            var dt = new DataTable("Menu_Hdr_Options");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqa = new SqlDataAdapter();
                // Define the select query and parameters
                var sql = @"Select *
                            FROM DDM_DynDBHdrConfig
                            WHERE DynDBMenuID = @DDM_MenuID
							AND HdrType = @Option_Type
                            ORDER BY SortOrder";

                // Create an array of SqlParameter objects
                var sqlparams = new SqlParameter[]
                {
					new SqlParameter("@DDM_MenuID", SqlDbType.Int) { Value = menu_option},
					new SqlParameter("@Option_Type", SqlDbType.Int) { Value = option_Type}
                };

                if (!String.IsNullOrEmpty(menu_option))
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }
 // BRApi.ErrorLog.LogMessage(si, $" Get Header {dt.Rows.Count}");
            return dt;
        }

    }
}