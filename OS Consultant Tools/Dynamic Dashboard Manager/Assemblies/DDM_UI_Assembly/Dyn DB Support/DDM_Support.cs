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
        public const string Param_DashboardMenu = "BL_DDM_AppMenu";

        private const string DefaultLayoutDashboardName = "DDM_App_Content_DB";
        private const string DefaultCubeViewName = "Default";

        public class DDM_PaneBinding
        {
            public DDM_ConfigHelpers.DBPaneContents ContentType { get; set; } = DDM_ConfigHelpers.DBPaneContents.Dashboard;
            public string DashboardName { get; set; } = DefaultLayoutDashboardName;
            public string CubeViewName { get; set; } = DefaultCubeViewName;
        }
		


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

        public static DataRow get_ConfigMenuRow(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            var menuOptionID = get_SelectedMenu(si, customSubstVars);
            var configMenuDt = get_ConfigMenu(si, menuOptionID);
            if (configMenuDt != null && configMenuDt.Rows.Count > 0)
            {
                return configMenuDt.Rows[0];
            }
            return null;
        }

        public static string get_LayoutDashboardName(DataRow configMenuRow)
        {
            if (configMenuRow == null)
            {
                return DefaultLayoutDashboardName;
            }

            int layoutType = GBL_UI_Assembly.GBL_Helpers.GetIntColumn(configMenuRow, "LayoutType", (int)DDM_ConfigHelpers.LayoutType.None);
            return (DDM_ConfigHelpers.LayoutType)layoutType switch
            {
                DDM_ConfigHelpers.LayoutType.Dashboard or
                DDM_ConfigHelpers.LayoutType.Dashboard_CustomDB    => GBL_UI_Assembly.GBL_Helpers.GetStringColumn(configMenuRow, "DB_Name", DefaultLayoutDashboardName),
                DDM_ConfigHelpers.LayoutType.CubeView              => "DDM_App_Content_CV",
                DDM_ConfigHelpers.LayoutType.Dashboard_TopBottom   => "DDM_App_Content_TB_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_LeftRight   => "DDM_App_Content_LR_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_2Top1Bottom => "DDM_App_Content_2T1B_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_1Top2Bottom => "DDM_App_Content_1T2B_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_2Left1Right => "DDM_App_Content_2L1R_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_1Left2Right => "DDM_App_Content_1L2R_DB",
                DDM_ConfigHelpers.LayoutType.Dashboard_2x2        => "DDM_App_Content_2x2_DB",
                _                                                  => DefaultLayoutDashboardName
            };
        }

        public static DDM_PaneBinding get_PaneBinding(SessionInfo si, DataRow configMenuRow, string dynamicDashboardName)
        {
            var paneBinding = new DDM_PaneBinding();
            if (configMenuRow == null)
            {
                return paneBinding;
            }

			BRApi.ErrorLog.LogMessage(si,$"Hit5");
            int layoutType = GBL_UI_Assembly.GBL_Helpers.GetIntColumn(configMenuRow, "LayoutType", (int)DDM_ConfigHelpers.LayoutType.None);
			BRApi.ErrorLog.LogMessage(si,$"Hit6");
            var paneName = dynamicDashboardName;
BRApi.ErrorLog.LogMessage(si,$"Hit7");
            if (dynamicDashboardName.XFEqualsIgnoreCase("DDM_App_Content_DB"))
            {
                paneBinding.DashboardName = get_LayoutDashboardName(configMenuRow);
                if (layoutType == (int)DDM_ConfigHelpers.LayoutType.CubeView)
                {
                    paneBinding.ContentType = DDM_ConfigHelpers.DBPaneContents.CubeView;
                    paneBinding.CubeViewName = GBL_UI_Assembly.GBL_Helpers.GetStringColumn(configMenuRow, "CV_Name", DefaultCubeViewName);
                }
                else
                {
                    paneBinding.ContentType = DDM_ConfigHelpers.DBPaneContents.Dashboard;
                    paneBinding.DashboardName = get_LayoutDashboardName(configMenuRow);
                }
                return paneBinding;
            }

            var contentType = resolve_PaneContentType(configMenuRow, paneName);
            paneBinding.ContentType = contentType;

            if (contentType == DDM_ConfigHelpers.DBPaneContents.CubeView)
            {
                paneBinding.CubeViewName = resolve_PaneName(configMenuRow, paneName, true);
            }
            else
            {
                paneBinding.DashboardName = resolve_PaneName(configMenuRow, paneName, false);
            }

            return paneBinding;
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

        private static string get_PaneName(string dynamicDashboardName)
        {
            const string prefix = "DDM_App_Content_";
            const string suffix = "DB";

			if (!string.IsNullOrEmpty(dynamicDashboardName)
			    && dynamicDashboardName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
			    && dynamicDashboardName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
			    && !dynamicDashboardName.Equals(prefix + suffix, StringComparison.OrdinalIgnoreCase)) // Ensures it's not just Prefix + Suffix
			{
			    return dynamicDashboardName.Substring(prefix.Length, dynamicDashboardName.Length - prefix.Length - suffix.Length);
			}
			else if (dynamicDashboardName.Equals(prefix + suffix, StringComparison.OrdinalIgnoreCase))
			{}

            return string.Empty;
        }

        private static DDM_ConfigHelpers.DBPaneContents resolve_PaneContentType(DataRow row, string paneName)
        {
            var typeColumnCandidates = new List<string>();

            switch (paneName.ToUpperInvariant())
            {
                case "T":
                    typeColumnCandidates.AddRange(new[] { "T_ContentType", "L_ContentType" });
                    break;
                case "B":
                    typeColumnCandidates.AddRange(new[] { "B_ContentType", "R_ContentType", "Right_Option_Type" });
                    break;
                case "L":
                    typeColumnCandidates.Add("L_ContentType");
                    break;
                case "R":
                    typeColumnCandidates.AddRange(new[] { "R_ContentType", "Right_Option_Type" });
                    break;
                case "TL":
                    typeColumnCandidates.AddRange(new[] { "TL_ContentType", "L_ContentType" });
                    break;
                case "TR":
                    typeColumnCandidates.AddRange(new[] { "TR_ContentType", "R_ContentType", "Right_Option_Type" });
                    break;
                case "BL":
                    typeColumnCandidates.AddRange(new[] { "BL_ContentType", "L_ContentType" });
                    break;
                case "BR":
                    typeColumnCandidates.AddRange(new[] { "BR_ContentType", "R_ContentType", "Right_Option_Type" });
                    break;
                default:
                    typeColumnCandidates.Add("LayoutType");
                    break;
            }

            foreach (var columnName in typeColumnCandidates)
            {
                int parsedType;
                if (GBL_UI_Assembly.GBL_Helpers.TryGetIntColumn(row, columnName, out parsedType))
                {
                    if (parsedType == (int)DDM_ConfigHelpers.DBPaneContents.CubeView
                        || parsedType == (int)DDM_ConfigHelpers.LayoutType.CubeView)
                    {
                        return DDM_ConfigHelpers.DBPaneContents.CubeView;
                    }
                    if (parsedType == (int)DDM_ConfigHelpers.DBPaneContents.Dashboard
                        || parsedType == (int)DDM_ConfigHelpers.LayoutType.Dashboard
                        || parsedType == (int)DDM_ConfigHelpers.LayoutType.Dashboard_CustomDB)
                    {
                        return DDM_ConfigHelpers.DBPaneContents.Dashboard;
                    }
                }
            }

            return DDM_ConfigHelpers.DBPaneContents.Dashboard;
        }

        private static string resolve_PaneName(DataRow row, string paneName, bool isCubeView)
        {
            var candidateColumns = new List<string>();

            switch (paneName.ToUpperInvariant())
            {
                case "T":
                    candidateColumns.AddRange(isCubeView
                        ? new[] { "T_Name", "CV_Name_Left", "CV_Name" }
                        : new[] { "T_Name", "DB_Name_Left", "DB_Name" });
                    break;
                case "B":
                    candidateColumns.AddRange(isCubeView
                        ? new[] { "B_Name", "CV_Name_Right", "CV_Name" }
                        : new[] { "B_Name", "DB_Name_Right", "DB_Name" });
                    break;
                case "L":
                case "TL":
                case "BL":
                    candidateColumns.AddRange(isCubeView
                        ? new[] { $"{paneName}_Name", "CV_Name_Left", "CV_Name" }
                        : new[] { $"{paneName}_Name", "DB_Name_Left", "DB_Name" });
                    break;
                case "R":
                case "TR":
                case "BR":
                    candidateColumns.AddRange(isCubeView
                        ? new[] { $"{paneName}_Name", "CV_Name_Right", "CV_Name" }
                        : new[] { $"{paneName}_Name", "DB_Name_Right", "DB_Name" });
                    break;
                default:
                    candidateColumns.Add(isCubeView ? "CV_Name" : "DB_Name");
                    break;
            }

            foreach (var columnName in candidateColumns)
            {
                var columnValue = GBL_UI_Assembly.GBL_Helpers.GetStringColumn(row, columnName, string.Empty);
                if (!string.IsNullOrEmpty(columnValue))
                {
                    return columnValue;
                }
            }

            return isCubeView ? DefaultCubeViewName : DefaultLayoutDashboardName;
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
