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

		// LayoutType (int) -> layout dashboard name.
		// Values 1 (Dashboard), 2 (CubeView) and 10 (CustomDB) are column-driven:
		// the actual name/view comes from the menu row (DB_Name / CV_Name), so they are
		// resolved from the passed-in column values rather than from this map.
		// Built as a Dictionary so it can later be repointed at a DB lookup table
		// (e.g. DDM_DynDBLayoutType) by changing only how this map is populated -
		// every call site stays "_layoutDashboards[layoutType]".
		private static readonly Dictionary<int, string> _layoutDashboards = new Dictionary<int, string>
		{
			{ 3, "DDM_App_Content_TB_DB"   }, // Dashboard_TopBottom
			{ 4, "DDM_App_Content_LR_DB"   }, // Dashboard_LeftRight
			{ 5, "DDM_App_Content_2T1B_DB" }, // Dashboard_2Top1Bottom
			{ 6, "DDM_App_Content_1T2B_DB" }, // Dashboard_1Top2Bottom
			{ 7, "DDM_App_Content_2L1R_DB" }, // Dashboard_2Left1Right
			{ 8, "DDM_App_Content_1L2R_DB" }, // Dashboard_1Left2Right
			{ 9, "DDM_App_Content_2x2_DB"  }  // Dashboard_2x2
		};

		// Mirrors DDM_ConfigHelpers.LayoutType for the column-driven cases.
		private const int LayoutType_None        = 0;
		private const int LayoutType_Dashboard   = 1;
		private const int LayoutType_CubeView    = 2;
		private const int LayoutType_CustomDB    = 10;

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
		/// Returns the content layout dashboard name for the menu the user is currently on.
		/// Determines the current menu from the dashboard substitution variables, reads that
		/// menu's LayoutType (and DB_Name / CV_Name) from DDM_DynDBMenuLayoutConfig, and maps
		/// it to a layout dashboard. Caller may override LayoutType / DB_Name / CV_Name via
		/// NameValuePairs; otherwise everything is sourced from the DB.
		/// </summary>
		private string Get_LayoutDB(SessionInfo si, DashboardStringFunctionArgs args)
		{
			int layoutType;
			var dbName = string.Empty;
			var cvName = string.Empty;

			// 1) Allow an explicit LayoutType override via NameValuePairs (testing / direct calls).
			var layoutTypeOverride = args.NameValuePairs.XFGetValue("LayoutType", string.Empty);

			if (!string.IsNullOrEmpty(layoutTypeOverride) && int.TryParse(layoutTypeOverride, out layoutType))
			{
				dbName = args.NameValuePairs.XFGetValue("DB_Name", string.Empty);
				cvName = args.NameValuePairs.XFGetValue("CV_Name", string.Empty);
			}
			else
			{
				// 2) DB-driven: figure out which menu (DB) we are currently on, then read its row.
				// A DashboardStringFunction receives dashboard values through NameValuePairs,
				// so the current menu id arrives as the BL_DDM_App_Menu pair (the dashboard
				// binds |!BL_DDM_App_Menu!| when it calls this function). Default to "1" to
				// match get_SelectedMenu's own default for the first/initial load.
				var selectedMenu = -1;
				var menuStr = args.NameValuePairs.XFGetValue(DDM_Support.Param_DashboardMenu, "1");
				if (!string.IsNullOrEmpty(menuStr) && !int.TryParse(menuStr, out selectedMenu))
				{
					selectedMenu = -1;
				}
				BRApi.ErrorLog.LogMessage(si, $"Get_Layout_Dashboard: current menu id = {selectedMenu}");

				var configMenuDT = DDM_Support.get_ConfigMenu(si, selectedMenu);
				if (configMenuDT == null || configMenuDT.Rows.Count == 0)
				{
					BRApi.ErrorLog.LogMessage(si, "Get_Layout_Dashboard: no config row found; returning default.");
					return DefaultDashboard;
				}

				var row = configMenuDT.Rows[0];

				// LayoutType is a non-null int column, but stay defensive.
				if (row["LayoutType"] == DBNull.Value || !int.TryParse(row["LayoutType"].ToString(), out layoutType))
				{
					BRApi.ErrorLog.LogMessage(si, "Get_Layout_Dashboard: LayoutType missing/invalid; returning default.");
					return DefaultDashboard;
				}

				if (configMenuDT.Columns.Contains("DB_Name") && row["DB_Name"] != DBNull.Value)
				{
					dbName = row["DB_Name"].ToString();
				}
				if (configMenuDT.Columns.Contains("CV_Name") && row["CV_Name"] != DBNull.Value)
				{
					cvName = row["CV_Name"].ToString();
				}
			}

			BRApi.ErrorLog.LogMessage(si, $"Get_Layout_Dashboard: LayoutType={layoutType}, DB_Name='{dbName}', CV_Name='{cvName}'");

			return Resolve_Layout_Dashboard(layoutType, dbName, cvName);
		}

		/// <summary>
		/// Pure mapping from a LayoutType (plus the row's DB_Name / CV_Name for the
		/// column-driven cases) to a layout dashboard name. No SI / DB dependency.
		/// </summary>
		private string Resolve_Layout_Dashboard(int layoutType, string dbName, string cvName)
		{
			// Column-driven cases: name comes from the menu row, not the map.
			if (layoutType == LayoutType_Dashboard || layoutType == LayoutType_CustomDB)
			{
				return string.IsNullOrEmpty(dbName) ? DefaultDashboard : dbName;
			}

			if (layoutType == LayoutType_CubeView)
			{
				// The CV shell hosts the cube view; the caller sets CubeViewName from CV_Name.
				return "DDM_App_Content_CV";
			}

			if (layoutType == LayoutType_None)
			{
				return DefaultDashboard;
			}

			// Fixed grid layouts (3-9).
			string dashboardName;
			return _layoutDashboards.TryGetValue(layoutType, out dashboardName)
				? dashboardName
				: DefaultDashboard;
		}
		#endregion
	}
}
