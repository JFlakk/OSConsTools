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
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_GBL_Command_Builder
    {
        private readonly SqlConnection _connection;

        public SQA_GBL_Command_Builder(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


       public void UpdateTable(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa)
        {
            _ = si;
            var builder = new GBL_SQL_Command_Builder(_connection, tableName, dt);

            var tableDef = builder.gettableDefsfromConfig(si, tableName);
            
            string[] FilterColumns(IEnumerable<string>? columns) =>
            (columns ?? Array.Empty<string>())
                .Where(dt.Columns.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
			
			
            var resolvedPrimaryKeys = FilterColumns(tableDef.PKs);
            if (resolvedPrimaryKeys.Length == 0)
            {
            throw new XFException("Primary key columns must exist in the DataTable.");
            }

            var updateExclusions = FilterColumns(tableDef.UpdEx);
            var insertExclusions = FilterColumns(tableDef.InsEx);

            using var transaction = _connection.BeginTransaction();
            try
            {

            builder.SetPrimaryKey(resolvedPrimaryKeys);

            if (updateExclusions.Length > 0)
            {
                builder.ExcludeFromUpdate(updateExclusions);
            }

            if (insertExclusions.Length > 0)
            {
                builder.ExcludeFromInsert(insertExclusions);
            }
			
            builder.ConfigureAdapter(sqa, transaction);
			
            sqa.Update(dt);
            transaction.Commit();

            }
            catch (Exception ex)
            {
            transaction.Rollback();
            throw new XFException(si, ex);
            }
            finally
            {
            sqa.InsertCommand = null;
            sqa.UpdateCommand = null;
            sqa.DeleteCommand = null;
            }
        }

        public GBL_SQL_Command_Builder GetCommandBuilder(SessionInfo si, string tableName, DataTable dt)
        {
            return new GBL_SQL_Command_Builder(_connection, tableName, dt);
        }

        /// <summary>
        /// Dynamically fill a DataTable from a SQL query with optional parameters
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="sqa">SqlDataAdapter to use for the Fill operation</param>
        /// <param name="dt">DataTable to fill with results</param>
        /// <param name="sql">SQL SELECT query to execute</param>
        /// <param name="sqlparams">Optional SQL parameters for the query</param>
        public void FillDataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams?.Length > 0)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Merge source DataTable into target DataTable with primary key preservation
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="targetDt">Target DataTable to merge into</param>
        /// <param name="sourceDt">Source DataTable to merge from</param>
        /// <param name="preserveChanges">True to preserve changes in the target; false to overwrite</param>
        /// <param name="missingSchemaAction">Action to take when schema doesn't match</param>
        public void MergeDataTable(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
            bool preserveChanges = true, MissingSchemaAction missingSchemaAction = MissingSchemaAction.Error)
        {
            try
            {
                targetDt.Merge(sourceDt, preserveChanges, missingSchemaAction);
            }
            catch (Exception ex)
            {
                throw new XFException($"Error merging DataTable: {ex.Message}");
            }
        }

        /// <summary>
        /// Merge source DataTable into target DataTable with composite primary key handling
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="targetDt">Target DataTable to merge into</param>
        /// <param name="sourceDt">Source DataTable to merge from</param>
        /// <param name="primaryKeyColumns">Array of column names that form the composite primary key</param>
        /// <param name="preserveChanges">True to preserve changes in the target; false to overwrite</param>
        public void MergeDataTableWithKeys(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
            string[] primaryKeyColumns, bool preserveChanges = false)
        {
            try
            {
                // Set primary keys on both tables if not already set
                if (targetDt.PrimaryKey == null || targetDt.PrimaryKey.Length == 0)
                {
                    var targetKeys = primaryKeyColumns.Select(col => 
                    {
                        if (targetDt.Columns[col] == null)
                            throw new ArgumentException($"Column '{col}' does not exist in target DataTable");
                        return targetDt.Columns[col];
                    }).ToArray();
                    targetDt.PrimaryKey = targetKeys;
                }

                if (sourceDt.PrimaryKey == null || sourceDt.PrimaryKey.Length == 0)
                {
                    var sourceKeys = primaryKeyColumns.Select(col => 
                    {
                        if (sourceDt.Columns[col] == null)
                            throw new ArgumentException($"Column '{col}' does not exist in source DataTable");
                        return sourceDt.Columns[col];
                    }).ToArray();
                    sourceDt.PrimaryKey = sourceKeys;
                }

                // Perform the merge
                targetDt.Merge(sourceDt, preserveChanges, MissingSchemaAction.Add);
            }
            catch (Exception ex)
            {
                throw new XFException($"Error merging DataTable with keys: {ex.Message}");
            }
        }
    }
}