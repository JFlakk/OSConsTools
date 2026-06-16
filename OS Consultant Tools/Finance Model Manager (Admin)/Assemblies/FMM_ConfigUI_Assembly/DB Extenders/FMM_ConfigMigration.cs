using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Config_Migration
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case DashboardExtenderFunctionType.LoadDashboard:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Load Dashboard logic here.
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
								loadDashboardTaskResult.ModifiedCustomSubstVars = null;
								return loadDashboardTaskResult;
							}
						}
						break;
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Dashboard Component Selection Changed logic here.
							var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
							selectionChangedTaskResult.IsOK = true;
							selectionChangedTaskResult.ShowMessageBox = false;
							selectionChangedTaskResult.Message = "";
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = null;
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVars = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = null;
							return selectionChangedTaskResult;
						}
						break;
					case DashboardExtenderFunctionType.SqlTableEditorSaveData:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement SQL Table Editor Save Data logic here.
							// Save the data rows.
							// XFSqlTableEditorSaveDataTaskInfo saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
							// using (DbConnInfo dbConn = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName))
							// {
								// dbConn.BeginTrans();
								// BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, true, false, false);
								// dbConn.CommitTrans();
							// }

							var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
							saveDataTaskResult.IsOK = true;
							saveDataTaskResult.ShowMessageBox = false;
							saveDataTaskResult.Message = "";
							saveDataTaskResult.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
							return saveDataTaskResult;
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
	}
}
