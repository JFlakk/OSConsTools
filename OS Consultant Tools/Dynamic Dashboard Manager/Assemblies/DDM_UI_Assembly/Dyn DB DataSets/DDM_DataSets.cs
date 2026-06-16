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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DDM_DataSets
{
    /// <summary>
    /// MainClass provides data retrieval methods for Dynamic Dashboard Manager datasets.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        #endregion

        /// <summary>
        /// Main entry point for the Dashboard DataSet business rule.
        /// Handles requests for dataset names and data retrieval based on function type and dataset name.
        /// </summary>
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        // Return available dataset names if needed
                        // var names = new List<string> { "MyDataSet" };
                        // return names;
                        break;
                    case DashboardDataSetFunctionType.GetDataSet:
                        // Return WF Profile Menu Options for selected profile
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_App_Menu"))
                        {
                            return Get_App_Menu();
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

		#region "Get App Menus"
        /// <summary>
        /// Retrieves the menu options for a given workflow profile.
        /// </summary>
        private DataTable Get_App_Menu()
        {
            try
            {
				//args.CustomSubstVars.
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
	// BRApi.ErrorLog.LogMessage(si, $"Here {wfUserPk.ProfileKey}");
                var dt = new DataTable("DDM_DynDBMenuConfig");
                // Define the SQL Statement
                var sql = @"Select Menu.DynDBMenuID, Menu.Name
		                    FROM DDM_DynDBConfig Cnfg
		                    JOIN DDM_DynDBMenuLayoutConfig Menu
		                    ON Cnfg.DynDBConfigID = Menu.DynDBConfigID
		                    WHERE Cnfg.WFPKey = @ProfileKey
		                    ORDER BY SortOrder";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
					connection.Open();
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
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
