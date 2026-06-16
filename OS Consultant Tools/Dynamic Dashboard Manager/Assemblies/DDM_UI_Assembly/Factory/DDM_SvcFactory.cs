using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_SvcFactory : IWsAssemblyServiceFactory
    {
        public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, WsAssemblyServiceType wsAssemblyServiceType, string itemName)
        {
            try
            {
                return wsAssemblyServiceType switch
                {
                    WsAssemblyServiceType.DynamicDashboards => new DDM_DynDBSvc(),
                    WsAssemblyServiceType.Dashboard => new DDM_DBSvc(),

                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}