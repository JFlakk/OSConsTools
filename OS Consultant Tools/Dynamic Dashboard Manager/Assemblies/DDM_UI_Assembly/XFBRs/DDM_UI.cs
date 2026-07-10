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
using Workspace.OSConsTools.DDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.DDM_UI
{
	public class MainClass
	{
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
        #endregion
		// Default returned when LayoutType is None/unmapped or required column value is missing.
		private const string DefaultDashboard = "DDM_App_Content_DB";

		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
		{
			try
			{
				this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
				if (args.FunctionName.XFEqualsIgnoreCase("Get_LayoutDB"))
				{
					return Get_LayoutDB();
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}

		#region "Layout Dashboard Resolver"
		private string Get_LayoutDB()
		{
			// 1) Allow an explicit LayoutType override via NameValuePairs (testing / direct calls).
			var layoutTypeOverride = args.NameValuePairs.XFGetValue("LayoutType", string.Empty);
			if (!string.IsNullOrEmpty(layoutTypeOverride) && int.TryParse(layoutTypeOverride, out int overrideLayoutType))
			{
				var dbName = args.NameValuePairs.XFGetValue("DB_Name", string.Empty);
				var cvName = args.NameValuePairs.XFGetValue("CV_Name", string.Empty);
				BRApi.ErrorLog.LogMessage(si, $"Get_LayoutDB: LayoutType override={overrideLayoutType}, DB_Name='{dbName}'");
				return Resolve_Layout_Dashboard(overrideLayoutType, dbName, cvName);
			}

			// 2) DB-driven: query DDM_DynDBMenuLayoutConfig for the currently selected menu.
			var configMenuRow = DDM_Support.get_ConfigMenuRow(si, args.NameValuePairs);
			if (configMenuRow == null)
			{
				BRApi.ErrorLog.LogMessage(si, "Get_LayoutDB: no config row found; returning default.");
				return DefaultDashboard;
			}

			// 3) Resolve the content dashboard name.
			//    Single-pane layouts (Dashboard, CubeView) always route through DDM_App_Content_DB;
			//    its dynamic service redirects CubeView content to DDM_App_Content_CV at render time.
			int layoutType = GBL_UI_Assembly.GBL_Helpers.GetIntColumn(configMenuRow, "LayoutType", (int)DDM_ConfigHelpers.LayoutType.None);
			var resolved = Resolve_Layout_Dashboard(layoutType, string.Empty, string.Empty);
			BRApi.ErrorLog.LogMessage(si, $"Get_LayoutDB: LayoutType={layoutType}, resolved='{resolved}'");
			return resolved;
		}

		/// <summary>
		/// Maps a LayoutType (plus optional DB_Name / CV_Name) to the dashboard name that
		/// <see cref="DDM_App_Content_C2"/> should embed.  Single-pane layouts always resolve
		/// to <c>DDM_App_Content_DB</c>; the dynamic dashboard service switches CubeView
		/// content to <c>DDM_App_Content_CV</c> at render time.
		/// </summary>
		private string Resolve_Layout_Dashboard(int layoutTypeInt, string dbName, string cvName)
		{
			switch ((DDM_ConfigHelpers.LayoutType)layoutTypeInt)
			{
				case DDM_ConfigHelpers.LayoutType.Dashboard:
				case DDM_ConfigHelpers.LayoutType.Dashboard_CustomDB:
				case DDM_ConfigHelpers.LayoutType.CubeView:
				case DDM_ConfigHelpers.LayoutType.None:
					// Single-pane content: always route through DDM_App_Content_DB.
					// Its GetDynamicComponentsForDynamicDashboard handler will redirect
					// CubeView content to DDM_App_Content_CV and bind the cube view there.
					return DefaultDashboard;

				case DDM_ConfigHelpers.LayoutType.Dashboard_TopBottom:    return "DDM_App_Content_TB_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_LeftRight:    return "DDM_App_Content_LR_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_2Top1Bottom:  return "DDM_App_Content_2T1B_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_1Top2Bottom:  return "DDM_App_Content_1T2B_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_2Left1Right:  return "DDM_App_Content_2L1R_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_1Left2Right:  return "DDM_App_Content_1L2R_DB";
				case DDM_ConfigHelpers.LayoutType.Dashboard_2x2:          return "DDM_App_Content_2x2_DB";

				default:
					return DefaultDashboard;
			}
		}
		#endregion
	}
}
