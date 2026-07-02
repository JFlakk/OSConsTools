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
						if (args.FunctionName.XFEqualsIgnoreCase("RunMigrations"))
						{
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								RunTableCalcMigrations(si);
								var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
								loadDashboardTaskResult.ModifiedCustomSubstVars = null;
								return loadDashboardTaskResult;
							}
						}
						else if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
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

		/// <summary>
		/// Adds any new columns required by the Table CalcType feature if they do not already exist.
		/// Safe to run multiple times (idempotent).
		/// </summary>
		private void RunTableCalcMigrations(SessionInfo si)
		{
			using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				// FMM_SrcCellConfig: Table_JoinType
				AddColumnIfMissing(si, dbConn, "FMM_SrcCellConfig", "Table_JoinType", "NVARCHAR(50)");

				// FMM_DestCell: DestTableName and CalcMode
				AddColumnIfMissing(si, dbConn, "FMM_DestCell", "DestTableName", "NVARCHAR(255)");
				AddColumnIfMissing(si, dbConn, "FMM_DestCell", "CalcMode", "NVARCHAR(50)");
			}
		}

		private void AddColumnIfMissing(SessionInfo si, DbConnInfoApp dbConn, string tableName, string columnName, string columnType)
		{
			var checkSql = @"
                SELECT COUNT(1)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @tableName
                  AND COLUMN_NAME = @columnName";

			var checkParams = new List<DbParamInfo>
			{
				new DbParamInfo("@tableName", tableName),
				new DbParamInfo("@columnName", columnName)
			};

			var exists = BRApi.Database.ExecuteScalar(dbConn, false, checkSql, checkParams);
			if (Convert.ToInt32(exists) == 0)
			{
				var alterSql = $"ALTER TABLE {tableName} ADD {columnName} {columnType} NULL";
				BRApi.Database.ExecuteActionQuery(dbConn, alterSql, null, false, true);
			}
		}
	}
}
