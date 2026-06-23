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


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_LoadDB
{
    public class MainClass
    {
        #region "Global Params"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private readonly GBL_Helpers gblHelpers = new GBL_Helpers();
        #endregion

        #region "Dictionary Setup"
        private string MainMenuParam = "BL_DDM_App_Menu";
        private string showHideIVName = "IV_DDM_App_Show_Hide_Menu_Btn";
        private string showBtnVisibleName = "IV_DDM_App_Display_Show_Menu_Btn";
        private string hideBtnVisibleName = "IV_DDM_App_Display_Hide_Menu_Btn";
        private string menuWidthIV = "IV_DDM_App_Menu_Width";
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("DDM_LoadDB"))
                        {
                            var loadDbTaskResult = LoadDB(ref args);
                            return loadDbTaskResult;
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

        #region "Class Helper Functions"
        #region "Load Dashboard"
        private XFLoadDashboardTaskResult LoadDB(ref DashboardExtenderArgs args)
        {
            var loadDbTaskResult = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true
            };

            setInitialParams(ref args, ref loadDbTaskResult);
            updateShowHide(ref args, ref loadDbTaskResult);
            setMenuOption(ref args, ref loadDbTaskResult);

            return loadDbTaskResult;
        }
        #endregion

        private void setInitialParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            bool isInitialLoad = args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize
                && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters;

            if (isInitialLoad)
            {
                UpdateCustomSubstVar(ref taskResult, showHideIVName, "Show");
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "Auto");
            }
        }

        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            var ARCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            string showHideIVVal = ARCustomSubst.XFGetValue(showHideIVName, "");

            if (showHideIVVal == "Hide")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "0");
            }
            else if (showHideIVVal == "Show")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "Auto");
            }
        }

        #region "Setup Helpers"
        private void setMenuOption(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var dt = new DataTable("WFP_Config");

            var menu_option = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(MainMenuParam, string.Empty);

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqa = new SqlDataAdapter();
                var sql = @"Select Menu.DynDBConfigID,Menu.DynDBMenuID,Menu.Name,
                            Menu.LayoutType, Menu.CustomDBHdr_Name,
                            Menu.CustomDBContent_Name,
                            Menu.DB_Name,Menu.CV_Name
                            FROM DDM_DynDBConfig Cnfg
                            JOIN DDM_DynDBMenuLayoutConfig Menu
                            ON Cnfg.DynDBConfigID = Menu.DynDBConfigID
                            WHERE Cnfg.WFPKey = @OS_WFProfileKey ";
                if (menu_option != string.Empty)
                {
                    sql += @"AND Menu.DynDBMenuID = @DDM_Menu_ID ";
                }

                sql += @"ORDER BY SortOrder";

                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@OS_WFProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUnitPk.ProfileKey }
                };

                if (!string.IsNullOrEmpty(menu_option))
                {
                    sqlparams = sqlparams.Append(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = Convert.ToInt32(menu_option) }).ToArray();
                }

                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                UpdateCustomSubstVar(ref taskResult, MainMenuParam, row["DynDBMenuID"].ToString());
            }
        }

        private void UpdateCustomSubstVar(ref XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
                globals.SetStringValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
                globals.SetStringValue(key, value);
            }
        }
        #endregion
        #endregion
    }
}
