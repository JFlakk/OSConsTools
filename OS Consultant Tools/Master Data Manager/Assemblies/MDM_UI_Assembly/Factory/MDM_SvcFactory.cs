using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;
using System;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class MDM_SvcFactory : IWsAssemblyServiceFactory
    {
        public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(
            SessionInfo si,
            BRGlobals brGlobals,
            DashboardWorkspace workspace,
            WsAssemblyServiceType wsAssemblyServiceType,
            string itemName)
        {
            try
            {
                return wsAssemblyServiceType switch
                {
                    WsAssemblyServiceType.DynamicDashboards => new MDM_DynDBSvc(),
                    WsAssemblyServiceType.Dashboard         => new MDM_DBSvc(),
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
