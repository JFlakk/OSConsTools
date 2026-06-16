using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
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
	public class FMM_ConfigHelpers
	{
		public interface IConfigMappings
		{
			Dictionary<int, Dictionary<string, string>> ParameterMappings { get; }
		}

		#region "Config Setup"
		#region "Cube Config"
		public class CubeConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CubeConfigRegistry
		{
			public static readonly Dictionary<SaveType, CubeConfig> Configs = new()
			{
				[SaveType.Add] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_CubeConfig_Name", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 5, new Dictionary<string, string> { { "DL_FMM_CubeConfig_Status", "Status" } } }
					}
				},
				[SaveType.Update] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_CubeConfig_Status", "Status" } } }
					}
				},
				[SaveType.View] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Name", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CubeConfig_CreateDate", "CreateDate" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CubeConfig_CreateUser", "CreateUser" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CubeConfig_UpdateDate", "UpdateDate" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CubeConfig_UpdateUser", "UpdateUser" } } }
					}
				}
			};
		}
		#endregion
		#region "Acct Config"
		public class AcctConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class AcctConfigRegistry
		{
			public static readonly Dictionary<SaveType, AcctConfig> Configs = new()
			{
				[SaveType.Add] = new AcctConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new AcctConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}
		#endregion
		#region "CustTableConfig Config"
		public class CustTableConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CustTableConfigRegistry
		{
			public static readonly Dictionary<SaveType, CustTableConfig> Configs = new()
			{
				[SaveType.Add] = new CustTableConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_FMM_CustTableConfig_Type", "Type" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Audit", "Audit" } } }
					}
				},
				[SaveType.Update] = new CustTableConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "Type" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "Audit" } } }
					}
				},
				[SaveType.View] = new CustTableConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_FMM_CustTableConfig_Type", "Type" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CustTableConfig_Audit", "Audit" } } }
					}
				}
			};
		}
		#endregion
		#region "CustTableConfig Assignment"

		#endregion
		#region "UI Config"
		
		#endregion
		#region "Approval Config"

		public class ApprConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class ApprConfigRegistry
		{
			public static readonly Dictionary<SaveType, ApprConfig> Configs = new()
			{
				[SaveType.Add] = new ApprConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new ApprConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}

		public class ApprStepConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class ApprStepConfigRegistry
		{
			public static readonly Dictionary<SaveType, ApprStepConfig> Configs = new()
			{
				[SaveType.Add] = new ApprStepConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new ApprStepConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}
		#endregion
		#region "Validation Config"
		public class ValConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}
		#endregion
		#region "Model Config"
		public class ModelConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class ModelConfigRegistry
		{
			public static readonly Dictionary<SaveType, ModelConfig> Configs = new()
			{
				[SaveType.Add] = new ModelConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_ModelConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_FMM_ModelConfig_CalcType", "CalcType" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_ModelConfig_ProcType", "ProcType" } } }
					}
				},
				[SaveType.Update] = new ModelConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_ModelConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_FMM_ModelConfig_CalcType", "CalcType" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_ModelConfig_ProcType", "ProcType" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_ModelConfig_Status", "Status" } } }
					}
				},
				[SaveType.View] = new ModelConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_ModelConfig_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_FMM_ModelConfig_CalcType", "CalcType" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_ModelConfig_ProcType", "ProcType" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_ModelConfig_Status", "Status" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_ModelConfig_CreateDate", "CreateDate" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_ModelConfig_CreateUser", "CreateUser" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_ModelConfig_UpdateDate", "UpdateDate" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_ModelConfig_UpdateUser", "UpdateUser" } } }
					}
				}
			};
		}
		#endregion
		#region "Calc Config"
		public class CalcConfig : IConfigMappings
		{
			public string DashboardName { get; init; }

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CalcRegistry
		{
			public static readonly Dictionary<CalcType, CalcConfig> Configs = new()
			{
				[CalcType.Table] = new CalcConfig
				{
					DashboardName = "FMM_CalcConfig_Table",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CalcConfig_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_CalcConfig_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.Cube] = new CalcConfig
				{
					DashboardName = "FMM_CalcConfig_Cube",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CalcConfig_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_CalcConfig_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.BRTabletoCube] = new CalcConfig
				{
					DashboardName = "FMM_ModelConfigBRTabletoCube",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CalcConfig_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_CalcConfig_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.CubetoTable] = new CalcConfig
				{
					DashboardName = "FMM_CalcConfig_BRCubetoTable",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CalcConfig_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_CalcConfig_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.Consolidate] = new CalcConfig
				{
					DashboardName = "FMM_CalcConfig_Consol",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_CalcConfig_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_CalcConfig_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_CalcConfig_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_CalcConfig_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_CalcConfig_UpdateUser", "UpdateUser" } } }
					}
				}
			};
		}

		public class DestConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class DestRegistry
		{
			public static readonly Dictionary<CalcType, DestConfig> Configs = new()
			{
				[CalcType.Table] = new DestConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Location", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CalcConfig_MultiDimAlloc", "MultiDim_Alloc" } } }
					}
				},
				[CalcType.Cube] = new DestConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Dest_Cons", "Cons" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Dest_View", "View" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Dest_Acct", "Acct" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Dest_Flow", "Flow" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Dest_IC", "IC" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Dest_Origin", "Origin" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Dest_UD1", "UD1" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Dest_UD2", "UD2" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Dest_UD3", "UD3" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Dest_UD4", "UD4" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Dest_UD5", "UD5" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Dest_UD6", "UD6" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Dest_UD7", "UD7" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Dest_UD8", "UD8" } } }
					}
				},
				[CalcType.CubetoTable] = new DestConfig
			    {
			        ParameterMappings = new()
			        {
			            //Added - Devlin
			            { 0, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Location", "Sequence" } } },
			            { 1, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Filter", "FilterExpression" } } },
			            { 2, new Dictionary<string, string> { { "IV_FMM_CalcConfig_Name", "Name" } } }
			        }
			    },
				[CalcType.BRTabletoCube] = new DestConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
					}
				}
			};
		}

		public class SrcConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class SrcRegistry
		{
			public static readonly Dictionary<CalcType, SrcConfig> Configs = new()
			{
				[CalcType.Table] = new SrcConfig
				{
					ParameterMappings = new()
					{
						//Added - Devlin
						{ 0, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrcOrder", "SrcOrder" } } },
				        { 1, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableSrcType", "Type" } } },
				        { 2, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrcItem", "Item" } } },
//				        { 3, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableCalcExpr", "CalcExpression" } } },
//				        { 4, new Dictionary<string, string> { { "IV_FMM_SrcConfig_JoinExpr", "JoinExpression" } } },
//				        { 5, new Dictionary<string, string> { { "IV_FMM_SrcConfig_FilterExpr", "FilterExpr" } } },
				        { 3, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapType", "MapType" } } },
				        { 4, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapSource", "MapSource" } } },
				        { 5, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapLogic", "MapLogic" } } }

					}
				},
				[CalcType.BRTabletoCube] = new SrcConfig
				{
					ParameterMappings = new()
					{	
						//Added - Devlin
						{ 0, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrcOrder", "SrcOrder" } } },
			            { 1, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableSrcType", "Type" } } },
			            { 2, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrceItem", "Item" } } },
			            { 3, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableCalcExpr", "CalcExpression" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_SrcConfig_JoinExpr", "JoinExpression" } } },
				        { 5, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapType", "MapType" } } },
				        { 6, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapSource", "MapSource" } } },
				        { 7, new Dictionary<string, string> { { "IV_FMM_SrcConfig_MapLogic", "MapLogic" } } }
					} 
				},
				
				[CalcType.CubetoTable] = new SrcConfig
				{
					ParameterMappings = new()
					{	
						//Added - Devlin
						{ 0, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrcOrder", "SrcOrder" } } },
				        { 1, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableSrcType", "Type" } } },
				        { 2, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Entity", "Entity" } } },
				        { 3, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Scenario", "Scenario" } } },
				        { 4, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Time", "Time" } } },
				        { 5, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Account", "Account" } } }
					}
				},
				[CalcType.Cube] = new SrcConfig
				{
				    ParameterMappings = new()
				    {	
						//Added - Devlin
//				        { 0, new Dictionary<string, string> { { "IV_FMM_SrcConfig_SrcOrder", "SrcOrder" } } },
//				        { 1, new Dictionary<string, string> { { "IV_FMM_SrcConfig_TableSrcType", "Type" } } },
				        { 0, new Dictionary<string, string> { { "DL_FMM_Src_CubeType", "CubeType" } } },
				        { 1, new Dictionary<string, string> { { "IV_FMM_SrcCellConficOpenParen", "OpenParen" } } },
				        { 2, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Entity", "Entity" } } },
				        { 3, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Cons", "Cons" } } },
				        { 4, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Scenario", "Scenario" } } },
				        { 5, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Time", "Time" } } },
				        { 6, new Dictionary<string, string> { { "IV_FMM_SrcConfig_View", "View" } } },
				        { 7, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Origin", "Origin" } } },
				        { 8, new Dictionary<string, string> { { "IV_FMM_SrcConfig_Flow", "Flow" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD1", "UD1" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD2", "UD2" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD3", "UD3" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD4", "UD4" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD5", "UD5" } } },
						{ 14, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD6", "UD6" } } },
						{ 15, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD7", "UD7" } } },
						{ 16, new Dictionary<string, string> { { "IV_FMM_SrcConfig_UD8", "UD8" } } },
				        { 17, new Dictionary<string, string> { { "IV_FMM_SrcCellConficCloseParen", "CloseParen" } } }
				    }
				},
				
			};
		}
		#endregion
		public class RegConfig : IConfigMappings
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class RegConfigRegistry
		{
			public static readonly Dictionary<SaveType, RegConfig> Configs = new()
			{
				[SaveType.Add] = new RegConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new RegConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}
		#endregion


		public static void MapConfigValues(DataRow row, Dictionary<string, string> substVars, IConfigMappings config)
		{
			if (config == null) return;

			foreach (var mapping in config.ParameterMappings.Values)
				foreach (var kvp in mapping)
					row[kvp.Value] = substVars.ContainsKey(kvp.Key) ? substVars[kvp.Key] : string.Empty;
		}

		public static CalcConfig Get_CalcConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (CalcRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public static DestConfig Get_DestConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (DestRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public static SrcConfig Get_SrcConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (SrcRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public static List<string> GetEnabledSrcProperties(int calctypeintValue)
		{
			var properties = new List<string>();
			var srcConfig = Get_SrcConfigType(calctypeintValue);

			if (srcConfig != null && srcConfig.ParameterMappings != null)
			{
				foreach (var mapping in srcConfig.ParameterMappings.Values)
				{
					foreach (var propertyName in mapping.Values)
					{
						if (!properties.Contains(propertyName) && !string.IsNullOrEmpty(propertyName))
						{
							properties.Add(propertyName);
						}
					}
				}
			}

			return properties;
		}
		public static List<string> GetEnabledDestProperties(int calctypeintValue)
		{
			var properties = new List<string>();
			var destConfig = Get_DestConfigType(calctypeintValue);

			if (destConfig != null && destConfig.ParameterMappings != null)
			{
				foreach (var mapping in destConfig.ParameterMappings.Values)
				{
					foreach (var propertyName in mapping.Values)
					{
						if (!properties.Contains(propertyName) && !string.IsNullOrEmpty(propertyName))
						{
							properties.Add(propertyName);
						}
					}
				}
			}

			return properties;
		}

		public static List<FMM_SrcCellModel> BuildSrcCellModels(
			IDictionary<string, string> customSubstVarsWithUserSelectedValues,
			string componentId,
			string dynamicSuffix,
			Func<string, string, string> getDynamicParamValue,
			Func<string, Guid> getDynamicParamGuid)
		{
			try
			{
				var cellModels = new List<FMM_SrcCellModel>();

				if (customSubstVarsWithUserSelectedValues == null)
				{
					return cellModels;
				}

				var calcTypeInt = 0;
				if (customSubstVarsWithUserSelectedValues.TryGetValue("DL_FMM_ModelConfig_CalcType", out var calcTypeValue))
				{
					int.TryParse(calcTypeValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out calcTypeInt);
				}

				var srcConfig = Get_SrcConfigType(calcTypeInt);
				if (srcConfig?.ParameterMappings == null)
				{
					return cellModels;
				}

				foreach (var mapping in srcConfig.ParameterMappings.Values)
				{
					var paramName = mapping.Keys.FirstOrDefault();
					if (string.IsNullOrWhiteSpace(paramName))
					{
						continue;
					}

					customSubstVarsWithUserSelectedValues.TryGetValue(paramName, out var defaultValue);

					var dynamicParamName = $"{paramName}{dynamicSuffix}";
					var model = new FMM_SrcCellModel();
					SetPropertyIfExists(model, new[] { "DynamicParamGuid", "DynamicParameterGuid", "ParamGuid" }, getDynamicParamGuid(dynamicParamName));
					SetPropertyIfExists(model, new[] { "DynamicParamName", "DynamicParameterName", "ParamName" }, dynamicParamName);
					SetPropertyIfExists(model, new[] { "Value", "ParamValue", "DynamicParameterValue" }, getDynamicParamValue(paramName, defaultValue ?? string.Empty));
					cellModels.Add(model);
				}

				return cellModels;
			}
			catch (Exception ex)
			{
				throw new XFException(ex.Message, ex);
			}
		}

		public static void SetConfigParams(SessionInfo si, ref Dictionary<string, string> substVars, string addUpdateFlag, string idSubstVar, string sql, string sqlParamName, string tableName, Func<SaveType, IConfigMappings> getConfig, IDictionary<string, string> idColumnToLookupSubstVar = null)
		{
			int idInt;
			if (addUpdateFlag != null)
			{
				var isUpdate = substVars.XFGetValue(addUpdateFlag, "NA").XFEqualsIgnoreCase("Update");
				idInt = isUpdate ? substVars.XFGetValue(idSubstVar, "0").XFConvertToInt() : 0;
				if (!isUpdate)
					GBL_UI_Assembly.GBL_Helpers.DictKeyAddUpdate(ref substVars, idSubstVar, string.Empty);
			}
			else
			{
				idInt = substVars.XFGetValue(idSubstVar, "0").XFConvertToInt();
			}

			var dt = new DataTable(tableName);
			try
			{
				var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
				using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
				{
					var gbl_dataset_helpers = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
					var sqa = new SqlDataAdapter();
					var sqlParams = new[] { new SqlParameter(sqlParamName, SqlDbType.Int) { Value = idInt } };
					gbl_dataset_helpers.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlParams);
				}
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}

			var config = getConfig(SaveType.View);
			if (config == null) return;

			var dataRow = dt.Rows.Count > 0 ? dt.Rows[0] : null;
			foreach (var mapping in config.ParameterMappings.Values)
				foreach (var kvp in mapping)
				{
					var rawValue = dataRow != null ? dataRow[kvp.Value].ToString() : string.Empty;

					// If this column has a registered lookup substVar, resolve the ID to its display text.
					var resolvedValue = rawValue;
					if (idColumnToLookupSubstVar != null && idColumnToLookupSubstVar.TryGetValue(kvp.Value, out var lookupSubstVar))
						resolvedValue = substVars.XFGetValue(lookupSubstVar, rawValue);

					GBL_UI_Assembly.GBL_Helpers.DictKeyAddUpdate(ref substVars, kvp.Key, resolvedValue);
				}
		}

		public static void SetCubeConfigParams(SessionInfo si, ref Dictionary<string, string> substVars)
		{
			var idColumnToLookupSubstVar = new Dictionary<string, string>
			{
				{ "Cube", "BL_FMM_All_Cube_Names" }   
			};
			SetConfigParams(si, ref substVars, "IV_FMM_CubeConfig_AddUpdate", "BL_FMM_CubeConfigID", "SELECT * FROM FMM_CubeConfig WHERE CubeConfigID = @CubeConfigID", "@CubeConfigID", "FMM_CubeConfig", st => CubeConfigRegistry.Configs.GetValueOrDefault(st), idColumnToLookupSubstVar);
		}

		public static void SetCustTableConfigParams(SessionInfo si, ref Dictionary<string, string> substVars)
		{
			SetConfigParams(si, ref substVars, "IV_FMM_CustTableConfig_AddUpdate", "BL_FMM_CustTableConfigID", "SELECT * FROM FMM_CustTableConfig WHERE CustTableConfigID = @CustTableConfigID", "@CustTableConfigID", "FMM_CustTableConfig", st => CustTableConfigRegistry.Configs.GetValueOrDefault(st));
		}

		public static void SetModelConfigParams(SessionInfo si, ref Dictionary<string, string> substVars)
		{
			SetConfigParams(si, ref substVars, "IV_FMM_ModelConfig_AddUpdate", "BL_FMM_ModelConfigID", "SELECT * FROM FMM_ModelConfig WHERE ModelConfigID = @ModelConfigID", "@ModelConfigID", "FMM_ModelConfig", st => ModelConfigRegistry.Configs.GetValueOrDefault(st));
		}

		private static void SetPropertyIfExists(object target, IEnumerable<string> candidateNames, object value)
		{
			if (target == null)
			{
				return;
			}

			foreach (var candidate in candidateNames)
			{
				var prop = target.GetType().GetProperty(candidate);
				if (prop != null && prop.CanWrite)
				{
					prop.SetValue(target, value);
					break;
				}
			}
		}

		public enum CalcType
		{
			None = 0,
			Table = 1,
			Cube = 2,
			BRTabletoCube = 3,
			CubetoTable = 4,
			Consolidate = 5
		}

		public enum SaveType
		{
			None = 0,
			Add = 1,
			Update = 2,
			View = 3
		}

		public enum CustTableConfigType
		{
			None = 0,
			Add = 1,
			Update = 2,
			View = 3
		}
	}
}