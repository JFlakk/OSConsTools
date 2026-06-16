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
    public class FMM_DynDBSvc : IWsasDynamicDashboardsV800
    {
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    if (storedDashboard.Name.XFEqualsIgnoreCase("FMM_SrcCellConfig_Cube_R2"))
                    {
                        var calcConfigID = customSubstVarsAlreadyResolved.XFGetValue("BL_FMM_CalcConfigID", "0").XFConvertToInt();
                        var src_CellDB = new FMM_SrcCellDB(si);
                        var src_Cells = src_CellDB.GetSrcCellsByCalcConfigID(calcConfigID, 1);

                        var repeatArgs = new List<WsDynamicComponentRepeatArgs>();

                        foreach (FMM_SrcCellModel cellModel in src_Cells)
                        {
                            var nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>
                            {
                                ["SrcCellConfigID"] = cellModel.srcCellConfigID.ToString(),
                                ["CalcConfigID"] = cellModel.calcConfigID.ToString(),
                                ["Entity"] = cellModel.entity,
                                ["Cons"] = cellModel.cons,
                                ["Scenario"] = cellModel.scenario,
                                ["Time"] = cellModel.time,
                                ["View"] = cellModel.view,
                                ["Acct"] = cellModel.acct,
                                ["IC"] = cellModel.ic,
                                ["Origin"] = cellModel.origin,
                                ["Flow"] = cellModel.flow,
                                ["UD1"] = cellModel.ud1,
                                ["UD2"] = cellModel.ud2,
                                ["UD3"] = cellModel.ud3,
                                ["UD4"] = cellModel.ud4,
                                ["UD5"] = cellModel.ud5,
                                ["UD6"] = cellModel.ud6,
                                ["UD7"] = cellModel.ud7,
                                ["UD8"] = cellModel.ud8,
                            };

                            repeatArgs.Add(new WsDynamicComponentRepeatArgs(
                                cellModel.srcCellConfigID.ToString(),
                                nextLevelTemplateSubstVarsToAdd));
                        }

                        WsDynamicDashboardEx contentDashboard = api.GetEmbeddedDynamicDashboard(si,
                            workspace, parentDynamicComponentEx, storedDashboard, string.Empty,
                            null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                        // attach our List of repeaters 
                        contentDashboard.DynamicDashboard.Tag = repeatArgs;

                        // save the state and return the dashboard
                        api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, contentDashboard, WsDynamicItemStateType.EntireObject);
                        if (contentDashboard.DynamicDashboard.Dashboard != null)
                            return contentDashboard;
                        return null;
                    }
                    else
                    {
                        return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicComponentCollection GetDynamicComponentsForDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    if (dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("FMM_SrcCellConfig_Cube_R2"))
                    {
                        var repeatArgsList = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
                        return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx,
                            repeatArgsList, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                    }
                    return api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicAdapterCollection GetDynamicAdaptersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicCubeViewEx GetDynamicCubeViewForDynamicAdapter(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicAdapterEx dynamicAdapterEx, CubeViewItem storedCubeViewItem, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx, storedCubeViewItem, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicParameterCollection GetDynamicParametersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.FalseValue, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}