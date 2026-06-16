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
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        #endregion

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
                        break;
                    case DashboardDataSetFunctionType.GetDataSet:
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_RootWFP"))
                        {
                            return Get_RootWFP();
                        }
						else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFPScenTypes"))
                        {
                            return Get_WFPScenTypes();
                        }	
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFP_trv"))
                        {
                            return Get_WFP_trv();
                        }
                        // Return WF Profile Hierarchy for selected root profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WSMU_TreeView"))
                        {
                            return Get_WSMU_TreeView();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WSMU_Dashboards"))
                        {
                            //return Get_WSMU_DB_TreeView();
                        }
                        // Return WF Profile Menu Options for selected profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_Menus"))
                        {
                            return Get_WFProfile_Menus();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFPNames"))
                        {
                            return Get_WFPNames();
                        }
                        // Return WF Profile Header Items for selected profile/menu option
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_Headers"))
                        {
                            return Get_WFProfile_Headers();
                        }
                        // Return WF Profile Header Items for selected profile/menu option
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_ConfigMenu"))
                        {
                            return Get_ConfigMenu();
                        }						
						else if (args.DataSetName.XFEqualsIgnoreCase("Get_Config_Hdrs"))
                        {
                            return Get_HdrConfig();
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

        #region "Get WF Profiles"
        private DataTable Get_RootWFP()
        {
            try
            {
                // Define the SQL Statement
                var sql = @"
                    SELECT ProfileName
                    FROM WorkflowProfileHierarchy
                    WHERE HierarchyLevel = 1
                        AND IsTemplate = 0 ";

                // Return the DataTable
                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, sql, false);
                    dt.TableName = "RootWFP";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion
        private DataTable Get_WFPScenTypes()
        {
            try
            {
				var dt = new DataTable();
				dt.Columns.Add("Key",typeof(int));
				dt.Columns.Add("Value",typeof(string));
				dt.TableName = "WFP_ScenTypes";
				var topWFP = args.NameValuePairs.XFGetValue("topWFP",String.Empty);
				var cubeList = BRApi.Finance.Cubes.GetTopLevelCubesForWorkflow(si);
				
				foreach (Cube topCube in cubeList)
				{
					var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, topCube.Name);
				    var scenTypeDict = topCubeInfo.GetScenarioTypesUsedByTopLevelCubeWFPName(si, topWFP);
					if (scenTypeDict != null)
		            {
		                foreach (var entry in scenTypeDict)
		                {
		                    dt.Rows.Add(entry.Key, entry.Value);
		                }
		            }
				}
				return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #region "Get WFProfile TreeView"

        /// <summary>
        /// Retrieves the workflow profile hierarchy as a tree view for the selected root profile.
        /// </summary>
        private DataSet Get_WFP_trv()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                string rootProfileName = args.CustomSubstVars.XFGetValue("BL_DDM_WFPRoot");

                var dt = new DataTable();

                // Define the SQL Statement
                var sql = @"
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
                        rcte.HierarchyIndex,
                        DDM.DynDBConfigID
                    FROM 
                        RecursiveCTE rcte
                    LEFT JOIN 
                        WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
                    LEFT JOIN
                        DDM_DynDBConfig DDM ON DDM.WFPKey = rcte.ProfileKey
                    ORDER BY 
                        rcte.HierarchyLevel DESC, 
                        rcte.HierarchyIndex";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                // Build the tree structure from the DataTable
                foreach (DataRow row in dt.Rows)
                {
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string profileName = row["ProfileName"].ToString();
                    string profileKey = row["ProfileKey"].ToString();
                    string parentprofileName = row["ParentProfileName"].ToString();
                    string parentprofileKey = row["ParentProfileKey"].ToString();
                    var Bold_WFProfile = true;
                    if (row["DynDBConfigID"] == DBNull.Value)
                    {
                        Bold_WFProfile = false;
                    }
                    parent_child.Add(profileKey, parentprofileKey);
                    var childProfiles = parent_child.Where(pair => pair.Value == profileKey)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {
                            // Create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.UniqueName == childProfile);
                            hierarchy_children_mbrs.Add(childXFTreeItem);
                        }
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
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
        #endregion
		
        #region "Get WFProfile TreeView"

        /// <summary>
        /// Retrieves the workflow profile hierarchy as a tree view for the selected root profile.
        /// </summary>
        private DataSet Get_WSMU_TreeView()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();

                var dt = new DataTable();

                // Define the SQL Statement
                var sql = @"
                        SELECT 
                            'All Maint Units' as Name,
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) as Value, 
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentNode,
							0 as Level
						UNION ALL
                        SELECT 
                            WS.Name,
                            WS.UniqueID as Value, 
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentNode,
							1 as Level
                        FROM 
                            DashboardWorkspace WS
						WHERE WS.Name NOT IN ('GENESIS','OS Consultant Tools','Default')
                        UNION ALL
                        SELECT 
                            MU.Name,
                            MU.UniqueID as Value, 
							MU.WorkspaceID as ParentNode,
						    2 as Level
                        FROM 
                            DashboardMaintUnit MU
                        INNER JOIN 
                            DashboardWorkspace WS ON MU.WorkspaceID = WS.UniqueID
	                    ORDER BY 
	                        Level DESC";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {

                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                // Build the tree structure from the DataTable
                foreach (DataRow row in dt.Rows)
                {
					BRApi.ErrorLog.LogMessage(si,row["Name"].ToString());
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string Name = row["Name"].ToString();
                    string Value = row["Value"].ToString();
                    string parentNode = row["ParentNode"].ToString();
                    var Bold_WFProfile = true;
//                    if (row["DDM_Config_ID"] == DBNull.Value)
//                    {
//                        Bold_WFProfile = false;
//                    }
                    parent_child.Add(Value, parentNode);
                    var childProfiles = parent_child.Where(pair => pair.Value == Value)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {
                            // Create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.UniqueName == childProfile);
							if (childXFTreeItem != null)
							{
                            	hierarchy_children_mbrs.Add(childXFTreeItem);
                        	}
							}
                        var wfprofile_xftreeitem = new XFTreeItem(Value, Name, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(Value, Name, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                }

                var hierXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == "All Maint Units");
                hierarchy.TreeItems.Add(hierXFTreeItem);
                return hierarchy.CreateDataSet(si);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
		
        private DataSet Get_WSMU_DB_TreeView()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();

                var dt = new DataTable();

                // Define the SQL Statement
                var sql = @"
                        SELECT 
                            'All Maint Units' as Name,
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) as Value, 
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentNode,
							0 as Level
						UNION ALL
                        SELECT 
                            WS.Name,
                            WS.UniqueID as Value, 
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentNode,
							1 as Level
                        FROM 
                            DashboardWorkspace WS
                        UNION ALL
                        SELECT 
                            MU.Name,
                            MU.UniqueID as Value, 
							MU.WorkspaceID as ParentNode,
						    2 as Level
                        FROM 
                            DashboardMaintUnit MU
                        INNER JOIN 
                            DashboardWorkspace WS ON MU.WorkspaceID = WS.UniqueID
	                    ORDER BY 
	                        Level DESC";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {

                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                // Build the tree structure from the DataTable
                foreach (DataRow row in dt.Rows)
                {
					BRApi.ErrorLog.LogMessage(si,row["Name"].ToString());
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string Name = row["Name"].ToString();
                    string Value = row["Value"].ToString();
                    string parentNode = row["ParentNode"].ToString();
                    var Bold_WFProfile = true;
//                    if (row["DDM_Config_ID"] == DBNull.Value)
//                    {
//                        Bold_WFProfile = false;
//                    }
                    parent_child.Add(Value, parentNode);
                    var childProfiles = parent_child.Where(pair => pair.Value == Value)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {
                            // Create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.UniqueName == childProfile);
							if (childXFTreeItem != null)
							{
                            	hierarchy_children_mbrs.Add(childXFTreeItem);
                        	}
							}
                        var wfprofile_xftreeitem = new XFTreeItem(Value, Name, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(Value, Name, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                }

                var hierXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == "All Maint Units");
                hierarchy.TreeItems.Add(hierXFTreeItem);
                return hierarchy.CreateDataSet(si);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion


        #region "Get WFProfile Menus"
        /// <summary>
        /// Retrieves the menu options for a given workflow profile.
        /// </summary>
        private DataTable Get_WFProfile_Menus()
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var WFProfile_Config_DT = new DataTable("WFProfile_Config");
                // Define the SQL Statement
                var sql = @"
                    Select Menu.DynDBMenuID, Menu.Name
                    FROM DDM_DynDBConfig Cnfg
                    JOIN DDM_DynDBMenuLayoutConfig Menu
                    ON Cnfg.DynDBConfigID = Menu.DynDBConfigID
                    WHERE Cnfg.WFPKey = @ProfileKey
                    ORDER BY SortOrder";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, WFProfile_Config_DT, sql, sqlparams);
                }
                return WFProfile_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        private DataTable Get_WFPNames()
        {
            try
            {
                // Define the SQL Statement
                var sql = @"
                    SELECT ProfileName, ProfileKey
                    FROM WorkflowProfileHierarchy
                    WHERE IsTemplate = 0 ";

                // Return the DataTable
                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, sql, false);
                    dt.TableName = "WFPNames";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
		
        #region "Get Config Menus"
        /// <summary>
        /// Retrieves the menu options for a given workflow profile.
        /// </summary>
        private DataTable Get_ConfigMenu()
        {
            try
            {
                var dynDBConfigID = args.NameValuePairs.XFGetValue("IV_DDM_ConfigID");
                var ddm_Config_Menu_DT = new DataTable("DDM_MenuLayoutConfig");
                // Define the SQL Statement
                var sql = @"SELECT CONCAT(SortOrder,'-',Name) as Name, DynDBMenuID as Value
							FROM DDM_DynDBMenuLayoutConfig
							WHERE DynDBConfigID = @DDM_ConfigID
							ORDER BY SortOrder";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("DDM_ConfigID", SqlDbType.Int) { Value = dynDBConfigID.XFConvertToInt()}
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, ddm_Config_Menu_DT, sql, sqlparams);
                }
                return ddm_Config_Menu_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
		
        private DataTable Get_HdrConfig()
        {
            try
            {
                var ddm_Menu_ID = args.NameValuePairs.XFGetValue("BL_DDM_MenuLayoutConfig","0");
				BRApi.ErrorLog.LogMessage(si,$"Menu_ID HDR: {ddm_Menu_ID}");
                var dt = new DataTable("DDM_Config_Hdrs");
                // Define the SQL Statement
                var sql = @"SELECT CONCAT(SortOrder,'-',Name) as Name, DynDBHdrID as Value
							FROM DDM_DynDBHdrConfig
							WHERE DynDBMenuID = @DDM_Menu_ID
							ORDER BY SortOrder";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("DDM_Menu_ID", SqlDbType.Int) { Value = ddm_Menu_ID.XFConvertToInt()}
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
		

        #region "Get WF Profile Headers"
        /// <summary>
        /// Retrieves header configuration options for a given workflow profile and menu option.
        /// </summary>
        private DataTable Get_WFProfile_Headers()
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var WFProfile_Header_Config_DT = new DataTable("WFProfile_Headers");
                string optionID = null;

                // Define the SQL Statement
                var sql = @"
                    SELECT *
                    FROM DDM_DynDBHdrConfig
                    WHERE DynDBConfigID = @ProfileKey
                    AND DynDBMenuID = @OptionID";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey and selected menu option 
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey },
                        new SqlParameter("@OptionID", SqlDbType.UniqueIdentifier) { Value = optionID }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, WFProfile_Header_Config_DT, sql, sqlparams);
                }
                return WFProfile_Header_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

		#region "Get Workspaces"
        /// <summary>
        /// Retrieves workspace configuration options for a given workflow profile and menu option.
        /// </summary>
        private DataTable Get_Workspaces()
        {
            try
            {
                var dt = new DataTable("Workspaces");

                // Define the SQL Statement
                var sql = @"SELECT Name, UniqueID
                            FROM DashboardWorkspace
                            WHERE Name NOT IN ('Genesis','OS Consultant Tools')";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey and selected menu option 
                    var sqlparams = new SqlParameter[]
                    {
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
