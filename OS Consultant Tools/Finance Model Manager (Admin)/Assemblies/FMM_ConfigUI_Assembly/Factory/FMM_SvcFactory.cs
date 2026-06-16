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
                return wsAssemblyServiceType switch
                {
                    WsAssemblyServiceType.DynamicDashboards => new FMM_DynDBSvc(),

                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}
