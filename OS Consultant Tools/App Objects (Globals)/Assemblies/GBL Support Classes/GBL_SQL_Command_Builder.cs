using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Generic SQL Command Builder that dynamically generates INSERT/UPDATE/DELETE commands
    /// based on DataTable schema. This eliminates hardcoded SQL statements and provides
    /// automatic synchronization when schema changes occur.
    /// </summary>
    public class GBL_SQL_Command_Builder
    {
        private readonly SqlConnection _connection;
        private readonly string _tableName;
        private readonly DataTable _dataTable;
        private readonly List<string> _primaryKeyColumns;
        private readonly List<string> _excludeFromUpdate;
        private readonly List<string> _excludeFromInsert;

        /// <summary>
        /// Initialize the SQL Command Builder
        /// </summary>
        /// <param name="connection">SQL connection to use</param>
        /// <param name="tableName">Name of the table to generate commands for</param>
        /// <param name="dataTable">DataTable with schema information</param>
        public GBL_SQL_Command_Builder(SqlConnection connection, string tableName, DataTable dataTable)
        {
            _connection = connection;
            _tableName = tableName;
            _dataTable = dataTable;
            _primaryKeyColumns = new List<string>();
            _excludeFromUpdate = new List<string>();
            _excludeFromInsert = new List<string>();
        }

        public (string[] PKs, string[] UpdEx, string[] InsEx) gettableDefsfromConfig(SessionInfo si, string tableName)
        {
            var tableDef = new Dictionary<string, (string[] PKs, string[] UpdEx, string[] InsEx)>(StringComparer.OrdinalIgnoreCase)
            {
				{ "FMM_ActConfig", (new[] { "ActConfigID" }, new[] { "ActConfigID", "CreateDate", "CreateUser" }, new[] { "ActConfigID" }) },
				{ "FMM_CubeConfig", (new[] { "CubeConfigID" }, new[] { "CubeConfigID", "CreateDate", "CreateUser" }, new[] { "CubeConfigID" }) },
                { "FMM_CustTable", (new[] { "CustTableID" }, new[] { "CustTableID", "CreateDate", "CreateUser" }, new[] { "CustTableID" }) },
                { "FMM_SrcCellConfig", (new[] { "SrcCellConfigID" }, new[] { "SrcCellConfigID", "CreateDate", "CreateUser" }, new[] { "SrcCellConfigID" }) },
                { "FMM_CustTableFK", (new[] { "CustTableFKID" }, new[] { "CustTableFKID", "CreateDate" }, new[] { "CustTableFKID" }) }
            };

            if (tableDef.TryGetValue(tableName, out var def))
            {
                return def;
            }
            else
            {
                // Call the fallback function to query the database
                return GetTableDefsFromMetadata(si, tableName);
            }
        }

        private (string[] PKs, string[] UpdEx, string[] InsEx) GetTableDefsFromMetadata(SessionInfo si, string tableName)
        {
            var pks = new List<string>();

            // Query SQL Server Schema for Primary Keys
            var sql = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_NAME = @tableName 
                AND OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1";

            using (var cmd = new SqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pks.Add(reader["COLUMN_NAME"].ToString());
                    }
                }
            }

            // Convert list to array
            string[] pkArray = pks.ToArray();

            // In a generic metadata fallback, we usually exclude PKs from both 
            // updates and inserts assuming they are Identity columns.
            return (pkArray, pkArray, pkArray);
        }

        public void SetPrimaryKey(params string[] columnNames)
        {
            _primaryKeyColumns.Clear();
            _primaryKeyColumns.AddRange(columnNames);
        }

        public void ExcludeFromUpdate(params string[] columnNames)
        {
            _excludeFromUpdate.Clear();
            _excludeFromUpdate.AddRange(columnNames);
        }

        /// <summary>
        /// Set columns to exclude from INSERT operations (e.g., identity columns)
        /// </summary>
        public void ExcludeFromInsert(params string[] columnNames)
        {
            _excludeFromInsert.Clear();
            _excludeFromInsert.AddRange(columnNames);
        }

        /// <summary>
        /// Build INSERT command based on DataTable schema
        /// </summary>
        public SqlCommand BuildInsertCommand(SqlTransaction transaction = null)
        {
            var columns = new List<string>();
            var parameters = new List<string>();

            foreach (DataColumn col in _dataTable.Columns)
            {
                if (_excludeFromInsert.Contains(col.ColumnName))
                    continue;

                columns.Add(col.ColumnName);
                parameters.Add($"@{col.ColumnName}");
            }

            var sql = $@"
                INSERT INTO {_tableName} (
                    {string.Join(", ", columns)}
                ) VALUES (
                    {string.Join(", ", parameters)}
                )";

            var command = transaction != null
                ? new SqlCommand(sql, _connection, transaction)
                : new SqlCommand(sql, _connection);

            command.UpdatedRowSource = UpdateRowSource.None;

            foreach (DataColumn col in _dataTable.Columns)
            {
                if (_excludeFromInsert.Contains(col.ColumnName))
                    continue;

                var param = command.Parameters.Add($"@{col.ColumnName}", GetSqlDbType(col.DataType));
                param.SourceColumn = col.ColumnName;

                if (col.MaxLength > 0 && (col.DataType == typeof(string)))
                {
                    param.Size = col.MaxLength;
                }
            }

            return command;
        }

        /// <summary>
        /// Build UPDATE command based on DataTable schema
        /// </summary>
        public SqlCommand BuildUpdateCommand(SqlTransaction transaction = null)
        {
            if (_primaryKeyColumns.Count == 0)
            {
                throw new InvalidOperationException("Primary key columns must be set before building UPDATE command");
            }

            var setClauses = new List<string>();
            var whereClauses = new List<string>();

            foreach (DataColumn col in _dataTable.Columns)
            {
                if (_primaryKeyColumns.Contains(col.ColumnName))
                {
                    whereClauses.Add($"{col.ColumnName} = @{col.ColumnName}");
                }
                else if (!_excludeFromUpdate.Contains(col.ColumnName))
                {
                    setClauses.Add($"{col.ColumnName} = @{col.ColumnName}");
                }
            }

            var sql = $@"
                UPDATE {_tableName} SET
                    {string.Join(",\n                    ", setClauses)}
                WHERE {string.Join(" AND ", whereClauses)}";

            var command = transaction != null
                ? new SqlCommand(sql, _connection, transaction)
                : new SqlCommand(sql, _connection);

            command.UpdatedRowSource = UpdateRowSource.None;

            // Add parameters for SET clause
            foreach (DataColumn col in _dataTable.Columns)
            {
                if (_primaryKeyColumns.Contains(col.ColumnName) || _excludeFromUpdate.Contains(col.ColumnName))
                    continue;

                var param = command.Parameters.Add($"@{col.ColumnName}", GetSqlDbType(col.DataType));
                param.SourceColumn = col.ColumnName;

                if (col.MaxLength > 0 && (col.DataType == typeof(string)))
                {
                    param.Size = col.MaxLength;
                }
            }

            // Add parameters for WHERE clause (primary keys)
            foreach (var pkColumn in _primaryKeyColumns)
            {
                var col = _dataTable.Columns[pkColumn];
                var param = command.Parameters.Add($"@{pkColumn}", GetSqlDbType(col.DataType));
                param.SourceColumn = pkColumn;
                param.SourceVersion = DataRowVersion.Original;

                if (col.MaxLength > 0 && (col.DataType == typeof(string)))
                {
                    param.Size = col.MaxLength;
                }
            }

            return command;
        }

        /// <summary>
        /// Build DELETE command based on primary key columns
        /// </summary>
        public SqlCommand BuildDeleteCommand(SqlTransaction transaction = null)
        {
            if (_primaryKeyColumns.Count == 0)
            {
                throw new InvalidOperationException("Primary key columns must be set before building DELETE command");
            }

            var whereClauses = new List<string>();

            foreach (var pkColumn in _primaryKeyColumns)
            {
                whereClauses.Add($"{pkColumn} = @{pkColumn}");
            }

            var sql = $@"
                DELETE FROM {_tableName}
                WHERE {string.Join(" AND ", whereClauses)}";

            var command = transaction != null
                ? new SqlCommand(sql, _connection, transaction)
                : new SqlCommand(sql, _connection);

            command.UpdatedRowSource = UpdateRowSource.None;

            foreach (var pkColumn in _primaryKeyColumns)
            {
                var col = _dataTable.Columns[pkColumn];
                var param = command.Parameters.Add($"@{pkColumn}", GetSqlDbType(col.DataType));
                param.SourceColumn = pkColumn;
                param.SourceVersion = DataRowVersion.Original;

                if (col.MaxLength > 0 && (col.DataType == typeof(string)))
                {
                    param.Size = col.MaxLength;
                }
            }

            return command;
        }

        /// <summary>
        /// Configure SqlDataAdapter with dynamically generated commands
        /// </summary>
        public void ConfigureAdapter(SqlDataAdapter adapter, SqlTransaction transaction = null)
        {
            adapter.InsertCommand = BuildInsertCommand(transaction);
            adapter.UpdateCommand = BuildUpdateCommand(transaction);
            adapter.DeleteCommand = BuildDeleteCommand(transaction);
            //adapter.UpdateBatchSize = ; // Set batch size for performance
        }

        /// <summary>
        /// Map .NET types to SQL Server types
        /// </summary>
        private SqlDbType GetSqlDbType(Type type)
        {
            if (type == typeof(int))
                return SqlDbType.Int;
            if (type == typeof(long))
                return SqlDbType.BigInt;
            if (type == typeof(short))
                return SqlDbType.SmallInt;
            if (type == typeof(byte))
                return SqlDbType.TinyInt;
            if (type == typeof(bool))
                return SqlDbType.Bit;
            if (type == typeof(DateTime))
                return SqlDbType.DateTime;
            if (type == typeof(decimal))
                return SqlDbType.Decimal;
            if (type == typeof(double))
                return SqlDbType.Float;
            if (type == typeof(float))
                return SqlDbType.Real;
            if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;
            if (type == typeof(byte[]))
                return SqlDbType.VarBinary;
            if (type == typeof(string))
                return SqlDbType.NVarChar;

            return SqlDbType.NVarChar; // Default fallback
        }
    }
}
