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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class FMM_SvcFactory : IWsAssemblyServiceFactory
	{
        public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(SessionInfo si, BRGlobals brGlobals,
            DashboardWorkspace workspace, WsAssemblyServiceType wsAssemblyServiceType, string itemName)
        {
            try
            {
                switch (wsAssemblyServiceType)
                {
                    //case WsAssemblyServiceType.Component:
                    //    return new WsasComponent();

                    //case WsAssemblyServiceType.Dashboard:
                    //    return new WsasDashboard();

                    //case WsAssemblyServiceType.DataManagementStep:
                    //    return new WsasDataManagementStep();

                    case WsAssemblyServiceType.DataSet:
                        return new FMM_DataSetSvc();

                    //case WsAssemblyServiceType.DynamicDashboards:
                    //    return new WsasDynamicDashboards();

                    //case WsAssemblyServiceType.DynamicCubeView:
                    //    return new WsasDynamicCubeView();

                    //case WsAssemblyServiceType.DynamicGrid:
                    //    return new WsasDynamicGrid();

                    //case WsAssemblyServiceType.FinanceCore:
                    //    return new WsasFinanceCore();

                    //case WsAssemblyServiceType.FinanceCustomCalculate:
                    //    return new WsasFinanceCustomCalculate();

                    //case WsAssemblyServiceType.FinanceGetDataCell:
                    //    return new WsasFinanceGetDataCell();

                    //case WsAssemblyServiceType.FinanceMemberLists:
                    //    return new WsasFinanceMemberLists();

                    //case WsAssemblyServiceType.SqlTableEditor:
                    //    return new WsasSqlTableEditor();

                    //case WsAssemblyServiceType.TableView:
                    //    return new WsasTableView();

                    //case WsAssemblyServiceType.XFBRString:
                    //    return new WsasXFBRString();

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
	}
}
