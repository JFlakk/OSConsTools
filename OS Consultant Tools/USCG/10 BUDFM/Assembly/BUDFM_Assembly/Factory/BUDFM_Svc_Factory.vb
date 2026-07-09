Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class BUDFM_Svc_Factory
		Implements IWsAssemblyServiceFactory

        Public Function CreateWsAssemblyServiceInstance(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal wsasType As WsAssemblyServiceType, ByVal itemName As String) As IWsAssemblyServiceBase Implements IWsAssemblyServiceFactory.CreateWsAssemblyServiceInstance
            Try
                Select Case wsasType
                    Case Is = WsAssemblyServiceType.Component
                        'Return New WsasComponent()

                    Case Is = WsAssemblyServiceType.Dashboard
                        'Return New WsasDashboard()

                    Case Is = WsAssemblyServiceType.DataManagementStep
                        'Return New WsasDataManagementStep()

                    Case Is = WsAssemblyServiceType.DataSet
                        'Return New WsasDataSet()

                    Case Is = WsAssemblyServiceType.DynamicDashboards
                    	'Return New WsasDynamicDashboards()

                    Case Is = WsAssemblyServiceType.DynamicCubeView
                    	'Return New WsasDynamicCubeView()

                    Case Is = WsAssemblyServiceType.DynamicGrid
                        'Return New WsasDynamicGrid()

                    Case Is = WsAssemblyServiceType.FinanceCore
                        'Return New WsasFinanceCore()

                    Case Is = WsAssemblyServiceType.FinanceCustomCalculate
                        Return New BUDFM_FinCustCalc()

                    Case Is = WsAssemblyServiceType.FinanceGetDataCell
                        'Return New WsasFinanceGetDataCell()

                    Case Is = WsAssemblyServiceType.FinanceMemberLists
						BRapi.ErrorLog.LogMessage(si,"Hit Svc")
                        Return New BUDFM_MbrLists()

                    Case Is = WsAssemblyServiceType.SqlTableEditor
                        'Return New WsasSqlTableEditor()

                    Case Is = WsAssemblyServiceType.TableView
                        'Return New WsasTableView()

                    Case Is = WsAssemblyServiceType.XFBRString
                    	'Return New WsasXFBRString()

                    Case Else
                        Return Nothing
                End Select

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

	End Class
End Namespace
