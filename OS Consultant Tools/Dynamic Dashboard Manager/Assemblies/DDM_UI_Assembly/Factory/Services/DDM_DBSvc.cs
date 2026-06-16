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
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_DBSvc : IWsasDashboardV800
    {
        public XFLoadDashboardTaskResult ProcessLoadDashboardTask(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args?.LoadDashboardTaskInfo != null))
                {
                    return null; //DDM_DB_Controller.GetLoadDashboardTaskResult(si, brGlobals, workspace, args);
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