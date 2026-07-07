using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class FMM_DataValidationEngine
    {
        public IReadOnlyList<string> ValidateDownstreamData(SessionInfo si, string targetType, DataTable data)
        {
            var errors = new List<string>();
            var configs = LoadValidationConfigs(si);

            foreach (var config in configs.Where(c => c.TargetType.XFEqualsIgnoreCase(targetType)))
            {
                var validationErrors = ValidateByType(config, data);
                foreach (var err in validationErrors)
                {
                    errors.Add($"[{config.Name}] {err}");
                }
            }

            return errors;
        }

        private static List<ValidationConfig> LoadValidationConfigs(SessionInfo si)
        {
            var configs = new List<ValidationConfig>();
            using var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using var connection = new SqlConnection(dbConnApp.ConnectionString);
            connection.Open();

            var dt = new DataTable("FMM_DataValConfig");
            var adapter = new SqlDataAdapter("SELECT * FROM FMM_DataValConfig", connection);
            adapter.Fill(dt);

            foreach (DataRow row in dt.Rows)
            {
                var status = GetColumnValue(row, "Status", "Active");
                var isActive = GetColumnValue(row, "IsActive", "1");
                if (!status.XFEqualsIgnoreCase("Active") && !status.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (isActive.XFEqualsIgnoreCase("0") || isActive.XFEqualsIgnoreCase("False"))
                {
                    continue;
                }

                var configJson = GetColumnValue(row, "ConfigJSON", string.Empty);
                var config = new ValidationConfig
                {
                    Name = GetColumnValue(row, "ValConfig", GetColumnValue(row, "Name", "Unnamed Validation")),
                    ValidationType = GetColumnValue(row, "ValType", GetColumnValue(row, "Type", "Required")),
                    Context = GetColumnValue(row, "Context", "Account"),
                    ConfigJson = configJson,
                    TargetType = ExtractJsonValue(configJson, "targetType", "Cube"),
                    ColumnName = ExtractJsonValue(configJson, "columnName", string.Empty),
                    Tolerance = ExtractJsonValue(configJson, "tolerance", "0")
                };
                configs.Add(config);
            }

            return configs;
        }

        private static List<string> ValidateByType(ValidationConfig config, DataTable data)
        {
            var errors = new List<string>();
            if (data == null || data.Rows.Count == 0)
            {
                return errors;
            }

            if (config.TargetType.XFEqualsIgnoreCase("Table"))
            {
                ValidateTable(config, data, errors);
            }
            else if (config.TargetType.XFEqualsIgnoreCase("Cube"))
            {
                ValidateCube(config, data, errors);
            }
            else if (config.TargetType.XFEqualsIgnoreCase("CubeToTable"))
            {
                ValidateCubeToTable(config, data, errors);
            }

            return errors;
        }

        private static void ValidateTable(ValidationConfig config, DataTable data, List<string> errors)
        {
            var columnName = ResolveColumn(data, config.ColumnName, config.Context, "Amount");
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            foreach (DataRow row in data.Rows)
            {
                var value = row[columnName]?.ToString() ?? string.Empty;
                if (config.ValidationType.XFEqualsIgnoreCase("Required") && string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Required value missing in column '{columnName}'.");
                }
                else if (config.ValidationType.XFEqualsIgnoreCase("Range"))
                {
                    if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        errors.Add($"Range validation expects numeric value in column '{columnName}'.");
                    }
                }
                else if (config.ValidationType.XFEqualsIgnoreCase("Pattern"))
                {
                    var pattern = ExtractJsonValue(config.ConfigJson, "pattern", string.Empty);
                    if (!string.IsNullOrWhiteSpace(pattern) && !Regex.IsMatch(value, pattern))
                    {
                        errors.Add($"Pattern validation failed for column '{columnName}'.");
                    }
                }
            }
        }

        private static void ValidateCube(ValidationConfig config, DataTable data, List<string> errors)
        {
            var columnName = ResolveColumn(data, config.ColumnName, config.Context, "Amount");
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            if (config.ValidationType.XFEqualsIgnoreCase("Required"))
            {
                foreach (DataRow row in data.Rows)
                {
                    var value = row[columnName]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        errors.Add($"Cube required validation failed for '{columnName}'.");
                    }
                }
            }
        }

        private static void ValidateCubeToTable(ValidationConfig config, DataTable data, List<string> errors)
        {
            var cubeCol = ResolveColumn(data, ExtractJsonValue(config.ConfigJson, "cubeColumn", string.Empty), "CubeAmount", "Amount");
            var tableCol = ResolveColumn(data, ExtractJsonValue(config.ConfigJson, "tableColumn", string.Empty), "TableAmount", "TargetAmount");
            if (string.IsNullOrEmpty(cubeCol) || string.IsNullOrEmpty(tableCol))
            {
                return;
            }

            decimal tolerance = 0m;
            decimal.TryParse(config.Tolerance, NumberStyles.Any, CultureInfo.InvariantCulture, out tolerance);

            foreach (DataRow row in data.Rows)
            {
                decimal.TryParse(row[cubeCol]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cubeVal);
                decimal.TryParse(row[tableCol]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var tableVal);
                if (Math.Abs(cubeVal - tableVal) > tolerance)
                {
                    errors.Add($"CubeToTable variance exceeded tolerance ({tolerance}) for '{cubeCol}' vs '{tableCol}'.");
                }
            }
        }

        private static string ResolveColumn(DataTable data, params string[] candidates)
        {
            foreach (var candidate in candidates.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                if (data.Columns.Contains(candidate))
                {
                    return candidate;
                }
            }
            return string.Empty;
        }

        private static string ExtractJsonValue(string json, string key, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return defaultValue;
            }

            var match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*\"(?<val>[^\"]*)\"", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["val"].Value : defaultValue;
        }

        private static string GetColumnValue(DataRow row, string column, string defaultValue)
        {
            if (row.Table.Columns.Contains(column))
            {
                return row[column]?.ToString() ?? defaultValue;
            }

            return defaultValue;
        }

        private sealed class ValidationConfig
        {
            public string Name { get; set; } = string.Empty;
            public string ValidationType { get; set; } = string.Empty;
            public string Context { get; set; } = string.Empty;
            public string ConfigJson { get; set; } = string.Empty;
            public string TargetType { get; set; } = string.Empty;
            public string ColumnName { get; set; } = string.Empty;
            public string Tolerance { get; set; } = string.Empty;
        }
    }
}
