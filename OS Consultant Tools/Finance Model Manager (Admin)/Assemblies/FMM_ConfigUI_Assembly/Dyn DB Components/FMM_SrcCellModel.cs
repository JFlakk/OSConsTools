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
    public class FMM_SrcCellModel
    {
        private readonly Dictionary<string, string> _dimensionValues;

        public FMM_SrcCellModel() : this(null)
        {
        }

        public FMM_SrcCellModel(IEnumerable<string> enabledDimensions)
        {
            type = openParens = mathOperator = closeParens = string.Empty;
            entity = cons = scenario = time = view = acct = ic = origin = flow = string.Empty;
            ud1 = ud2 = ud3 = ud4 = ud5 = ud6 = ud7 = ud8 = string.Empty;

            _dimensionValues = enabledDimensions?
                .ToDictionary(d => d, _ => string.Empty, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int cubeConfigID { get; set; }
        public int actConfigID { get; set; }
        public int modelConfigID { get; set; }
        public int calcConfigID { get; set; }
        public int srcCellConfigID { get; set; }
        public int srcOrder { get; set; }

        public string type { get; set; } = string.Empty;
        public string item { get; set; } = string.Empty;
        public string openParens { get; set; } = string.Empty;
        public string mathOperator { get; set; } = string.Empty;
        public string closeParens { get; set; } = string.Empty;

        public string entity { get; set; } = string.Empty;
        public string cons { get; set; } = string.Empty;
        public string scenario { get; set; } = string.Empty;
        public string time { get; set; } = string.Empty;
        public string view { get; set; } = string.Empty;
        public string acct { get; set; } = string.Empty;
        public string ic { get; set; } = string.Empty;
        public string origin { get; set; } = string.Empty;
        public string flow { get; set; } = string.Empty;
        public string ud1 { get; set; } = string.Empty;
        public string ud2 { get; set; } = string.Empty;
        public string ud3 { get; set; } = string.Empty;
        public string ud4 { get; set; } = string.Empty;
        public string ud5 { get; set; } = string.Empty;
        public string ud6 { get; set; } = string.Empty;
        public string ud7 { get; set; } = string.Empty;
        public string ud8 { get; set; } = string.Empty;


        // Dynamic accessor based on passed-in dimension list
        public string this[string dimension]
        {
            get => _dimensionValues.TryGetValue(dimension, out var value) ? value : string.Empty;
            set
            {
                if (_dimensionValues.ContainsKey(dimension))
                {
                    _dimensionValues[dimension] = value;
                }
            }
        }
    }
}