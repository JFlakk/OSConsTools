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
		private const string DefaultDashboard = "emb_Dynamic_DDM_App_Content_DB";

		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
		{
			try
			{
				if (args.FunctionName.XFEqualsIgnoreCase("SayHello"))
				{
					return "Hello";
				}

				// Resolve the content layout dashboard for the menu the user is currently on.
				// Self-contained: reads the selected menu from the dashboard's substitution
				// variables (BL_DDM_App_Menu) and looks up its LayoutType / DB_Name / CV_Name
				// from DDM_DynDBMenuLayoutConfig. No caller arguments required.
				//
				// Optional NameValuePairs overrides (handy for testing / explicit calls):
				//   LayoutType - integer matching DDM_ConfigHelpers.LayoutType
				//   DB_Name    - dashboard name for Dashboard(1) / CustomDB(10)
				//   CV_Name    - cube view name for CubeView(2)
				if (args.FunctionName.XFEqualsIgnoreCase("Get_LayoutDB"))
				{
					return Get_LayoutDB(si, args);
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}

		#region "Layout Dashboard Resolver"
		/// <summary>
		/// Returns the content layout dashboard name for the pane / dashboard that called this XFBR.
		/// <para>
		/// The caller passes <c>currDB</c> (the name of the dashboard calling this function) via
		/// NameValuePairs so the method knows which content area needs to be resolved.  The selected
		/// menu is read from <c>BL_DDM_App_Menu</c>, the DB is queried for that menu's config row,
		/// and <see cref="DDM_Support.get_PaneBinding"/> (which delegates to
		/// <see cref="DDM_ConfigHelpers"/>) maps the config to the correct dashboard name.
		/// </para>
		/// Optional NameValuePairs overrides (handy for testing / explicit calls):
		/// <list type="bullet">
		///   <item><c>LayoutType</c> – integer matching <see cref="DDM_ConfigHelpers.LayoutType"/></item>
		///   <item><c>DB_Name</c>   – dashboard name for Dashboard(1) / CustomDB(10)</item>
		///   <item><c>CV_Name</c>   – cube view name for CubeView(2)</item>
		/// </list>
		/// </summary>
		private string Get_LayoutDB(SessionInfo si, DashboardStringFunctionArgs args)
		{
			// 1) Identify which dashboard is calling this XFBR so multi-pane layouts can resolve
			//    the correct pane content.  Defaults to the top-level content DB.
			var currDB = args.NameValuePairs.XFGetValue("currDB", "DDM_App_Content_DB");

			// 2) Allow an explicit LayoutType override via NameValuePairs (testing / direct calls).
			var layoutTypeOverride = args.NameValuePairs.XFGetValue("LayoutType", string.Empty);
			if (!string.IsNullOrEmpty(layoutTypeOverride) && int.TryParse(layoutTypeOverride, out int overrideLayoutType))
			{
				var dbName = args.NameValuePairs.XFGetValue("DB_Name", string.Empty);
				var cvName = args.NameValuePairs.XFGetValue("CV_Name", string.Empty);
				BRApi.ErrorLog.LogMessage(si, $"Get_LayoutDB: LayoutType override={overrideLayoutType}, DB_Name='{dbName}', currDB='{currDB}'");
				return Resolve_Layout_Dashboard(overrideLayoutType, dbName, cvName);
			}

			// 3) DB-driven: query DDM_DynDBMenuLayoutConfig for the currently selected menu.
			var configMenuRow = DDM_Support.get_ConfigMenuRow(si, args.NameValuePairs);
			if (configMenuRow == null)
			{
				BRApi.ErrorLog.LogMessage(si, "Get_LayoutDB: no config row found; returning default.");
				return DefaultDashboard;
			}

			BRApi.ErrorLog.LogMessage(si, $"Get_LayoutDB: currDB='{currDB}'");

			// 4) Delegate to DDM_Support (which uses DDM_ConfigHelpers) to resolve the dashboard
			//    for the specific context (pane) that called this XFBR.
			var paneBinding = DDM_Support.get_PaneBinding(configMenuRow, currDB);
			BRApi.ErrorLog.LogMessage(si, $"Get_LayoutDB: resolved to '{paneBinding.DashboardName}'");
			return paneBinding.DashboardName;
		}

		/// <summary>
		/// Maps an explicit LayoutType override (plus optional DB_Name / CV_Name) to a layout
		/// dashboard name, using <see cref="DDM_ConfigHelpers.LayoutType"/> enum values.
		/// Only used for the NameValuePairs override path; the DB-driven path uses
		/// <see cref="DDM_Support.get_PaneBinding"/> instead.
		/// </summary>
		private string Resolve_Layout_Dashboard(int layoutTypeInt, string dbName, string cvName)
		{
			switch ((DDM_ConfigHelpers.LayoutType)layoutTypeInt)
			{
				case DDM_ConfigHelpers.LayoutType.Dashboard:
				case DDM_ConfigHelpers.LayoutType.Dashboard_CustomDB:
					return string.IsNullOrEmpty(dbName) ? DefaultDashboard : dbName;

				case DDM_ConfigHelpers.LayoutType.CubeView:
					// The CV shell hosts the cube view; the caller sets CubeViewName from CV_Name.
					return "DDM_App_Content_CV";

				case DDM_ConfigHelpers.LayoutType.None:
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
