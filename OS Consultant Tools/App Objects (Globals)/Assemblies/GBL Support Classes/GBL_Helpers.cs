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
    public class GBL_Helpers
    {
        public static void DictKeyAddUpdate(ref Dictionary<string, string> dict, string key, string value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
        public void UpdateCustomSubstVar(ref XFSelectionChangedTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
            }
        }

        public void UpdateCustomSubstVar(ref XFLoadDashboardTaskResult result, BRGlobals globals, string key, string value)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
            }

            globals?.SetStringValue(key, value);
        }

        public string checkBlankValue(ref DataRow row, string columnName, string rawValue)
        {
            if (!row.Table.Columns.Contains(columnName)) return "Error";

            var col = row.Table.Columns[columnName];
            var isEmpty = string.IsNullOrWhiteSpace(rawValue);

            if (isEmpty)
            {
                if (col.DataType == typeof(int))
                {
                    row[columnName] = 0;
                }
                else if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float))
                {
                    row[columnName] = 0.0;
                }
                else if (col.DataType == typeof(bool))
                {
                    row[columnName] = false;
                }
                else
                {
                    return "Error";
                }
				return "Default";
            }
            return "Success";
        }
    }
}