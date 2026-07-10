using System;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class MDM_DBSvc : IWsasDashboardV800
    {
        public XFLoadDashboardTaskResult ProcessLoadDashboardTask(
            SessionInfo si,
            BRGlobals brGlobals,
            DashboardWorkspace workspace,
            DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args?.LoadDashboardTaskInfo != null))
                {
                    return null;
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
