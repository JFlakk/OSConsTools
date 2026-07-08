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
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Core support class for the Master Data Manager end-user workspace.
    /// Provides menu resolution, pane binding, and shared query helpers.
    /// </summary>
    public class MDM_Support
    {
        #region "Constants"
        public const string Param_AppMenu     = "BL_MDM_AppMenu";
        public const string Param_CubeName    = "IV_MDM_App_CubeName";
        public const string Param_SelDim      = "IV_MDM_SelDimName";
        public const string Param_SelMember   = "IV_MDM_SelMemberName";
        public const string Param_SelChangeReq = "IV_MDM_SelChangeReqID";

        private const string DefaultLayoutDashboardName = "MDM_App_Content_DB";
        private const string DefaultCubeViewName        = "Default";
        #endregion

        #region "Pane Binding"
        public class MDM_PaneBinding
        {
            public MDM_ConfigHelpers.DBPaneContents ContentType   { get; set; } = MDM_ConfigHelpers.DBPaneContents.Dashboard;
            public string DashboardName                           { get; set; } = DefaultLayoutDashboardName;
            public string CubeViewName                            { get; set; } = DefaultCubeViewName;
        }

        /// <summary>
        /// Resolves the runtime pane binding (dashboard name or cube view name) for a dynamic
        /// dashboard pane, given the selected menu's config row and the calling pane name.
        /// Mirrors <c>DDM_Support.get_PaneBinding</c>.
        /// </summary>
        public static MDM_PaneBinding get_PaneBinding(SessionInfo si, DataRow configMenuRow, string dynamicDashboardName)
        {
            var paneBinding = new MDM_PaneBinding();
            if (configMenuRow == null)
            {
                return paneBinding;
            }

            int layoutType = GBL_UI_Assembly.GBL_Helpers.GetIntColumn(
                configMenuRow, "LayoutType", (int)MDM_ConfigHelpers.LayoutType.None);

            if (dynamicDashboardName.XFEqualsIgnoreCase("MDM_App_Content_DB"))
            {
                if (layoutType == (int)MDM_ConfigHelpers.LayoutType.CubeView)
                {
                    paneBinding.ContentType  = MDM_ConfigHelpers.DBPaneContents.CubeView;
                    paneBinding.CubeViewName = GBL_UI_Assembly.GBL_Helpers.GetStringColumn(
                        configMenuRow, "CV_Name", DefaultCubeViewName);
                }
                else
                {
                    paneBinding.ContentType   = MDM_ConfigHelpers.DBPaneContents.Dashboard;
                    paneBinding.DashboardName = get_LayoutDashboardName(configMenuRow);
                }
                return paneBinding;
            }

            paneBinding.ContentType   = MDM_ConfigHelpers.DBPaneContents.Dashboard;
            paneBinding.DashboardName = get_LayoutDashboardName(configMenuRow);
            return paneBinding;
        }
        #endregion

        #region "Menu Resolution"
        /// <summary>
        /// Returns the currently selected menu option ID from the subst var dictionary.
        /// </summary>
        public static int get_SelectedMenu(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            var menuOptionStr = customSubstVars.XFGetValue(Param_AppMenu, "1");
            return int.TryParse(menuOptionStr, out int menuOption) ? menuOption : 1;
        }

        /// <summary>
        /// Queries <c>MDM_MenuLayoutConfig</c> for the row that matches the selected menu option ID.
        /// </summary>
        public static DataRow get_ConfigMenuRow(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            var menuOptionID = get_SelectedMenu(si, customSubstVars);
            var dt           = get_ConfigMenu(si, menuOptionID);
            return (dt != null && dt.Rows.Count > 0) ? dt.Rows[0] : null;
        }

        private static DataTable get_ConfigMenu(SessionInfo si, int selectedMenu)
        {
            var dt     = new DataTable("MDM_MenuLayoutConfig_DT");
            if (selectedMenu < 0) return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper    = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, conn);
                var sqa       = new SqlDataAdapter();
                var sql       = @"
SELECT *
FROM   MDM_MenuLayoutConfig
WHERE  MenuOptionID = @MenuOptionID";
                var sqlparams = new[] { new SqlParameter("@MenuOptionID", SqlDbType.Int) { Value = selectedMenu } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        private static string get_LayoutDashboardName(DataRow configMenuRow)
        {
            if (configMenuRow == null) return DefaultLayoutDashboardName;

            int layoutType = GBL_UI_Assembly.GBL_Helpers.GetIntColumn(
                configMenuRow, "LayoutType", (int)MDM_ConfigHelpers.LayoutType.None);

            return (MDM_ConfigHelpers.LayoutType)layoutType switch
            {
                MDM_ConfigHelpers.LayoutType.Dashboard or
                MDM_ConfigHelpers.LayoutType.Dashboard_CustomDB    => GBL_UI_Assembly.GBL_Helpers.GetStringColumn(configMenuRow, "DB_Name", DefaultLayoutDashboardName),
                MDM_ConfigHelpers.LayoutType.CubeView              => "MDM_App_Content_CV",
                MDM_ConfigHelpers.LayoutType.Dashboard_TopBottom   => "MDM_App_Content_TB_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_LeftRight   => "MDM_App_Content_LR_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2Top1Bottom => "MDM_App_Content_2T1B_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_1Top2Bottom => "MDM_App_Content_1T2B_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2Left1Right => "MDM_App_Content_2L1R_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_1Left2Right => "MDM_App_Content_1L2R_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2x2        => "MDM_App_Content_2x2_DB",
                _                                                  => DefaultLayoutDashboardName
            };
        }
        #endregion

        #region "User Context"
        /// <summary>Returns the current user's MDM role for a given dimension.</summary>
        public static string get_UserRole(SessionInfo si, int dimConfigID)
        {
            var dt     = new DataTable("MDM_UserRole");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper    = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, conn);
                var sqa       = new SqlDataAdapter();
                var sql       = @"
SELECT ac.Role
FROM   MDM_AccessConfig ac
JOIN   MDM_DimConfig    dc ON dc.DimConfigID = ac.DimConfigID
WHERE  ac.DimConfigID = @DimConfigID
  AND  ac.Status      = 1
  AND  IS_MEMBER(ac.GroupName) = 1";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int) { Value = dimConfigID } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt.Rows.Count > 0 ? dt.Rows[0]["Role"]?.ToString() ?? string.Empty : string.Empty;
        }
        #endregion

        #region "Cube Name"
        public static string get_CubeName(SessionInfo si, int cubeId)
        {
            var dt     = new DataTable("Cubes");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper    = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, conn);
                var sqa       = new SqlDataAdapter();
                var sql       = @"SELECT Name FROM Cube WHERE CubeId = @CubeId";
                var sqlparams = new[] { new SqlParameter("@CubeId", SqlDbType.Int) { Value = cubeId } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt.Rows.Count > 0 ? dt.Rows[0]["Name"]?.ToString() ?? string.Empty : string.Empty;
        }
        #endregion
    }
}
