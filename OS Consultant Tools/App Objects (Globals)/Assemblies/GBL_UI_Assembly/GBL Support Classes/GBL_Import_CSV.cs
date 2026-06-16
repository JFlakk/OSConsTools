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
	public class GBL_Import_CSV
	{
	    private readonly string _connectionString;
	
	    public GBL_Import_CSV(string connectionString)
	    {
	        _connectionString = connectionString;
	    }
	
	    public void Import_CSV(string csvFilePath)
	    {
	        string tableName = Path.GetFileNameWithoutExtension(csvFilePath);
	
	        // 1. Read system tables for schema info
	        var schemaTable = GetTableSchema(tableName);
	        if (schemaTable.Rows.Count == 0)
	            throw new Exception($"Table '{tableName}' does not exist.");
	
	        // 2. Get primary keys
	        var primaryKeys = GetPrimaryKeys(tableName);
	
	        // 3. Read CSV
	        var csvLines = File.ReadAllLines(csvFilePath);
	        var csvColumns = csvLines[0].Split(',');
	
	        // 4. Validate columns
	        foreach (var col in csvColumns)
	        {
	            if (!schemaTable.AsEnumerable().Any(r => r["COLUMN_NAME"].ToString().Equals(col, StringComparison.OrdinalIgnoreCase)))
	                throw new Exception($"Column '{col}' does not exist in table '{tableName}'.");
	        }
	
	        // 5. Prepare DataTable for bulk copy
	        var dataTable = new DataTable();
	        foreach (var col in csvColumns)
	        {
	            var colType = schemaTable.AsEnumerable()
	                .First(r => r["COLUMN_NAME"].ToString().Equals(col, StringComparison.OrdinalIgnoreCase))["DATA_TYPE"].ToString();
	            dataTable.Columns.Add(col, GetTypeFromSqlType(colType));
	        }
	
	        // 6. Fill DataTable
	        foreach (var line in csvLines.Skip(1))
	        {
	            var values = line.Split(',');
	            dataTable.Rows.Add(values);
	        }
	
	        // 7. Merge/Upsert logic (simple example: delete and insert)
	        using (var conn = new SqlConnection(_connectionString))
	        {
	            conn.Open();
	            // Optional: Use transaction
	            using (var tran = conn.BeginTransaction())
	            {
	                // Example: Delete existing rows with matching PKs
	                foreach (DataRow row in dataTable.Rows)
	                {
	                    string whereClause = string.Join(" AND ", primaryKeys.Select(pk => $"{pk} = '{row[pk]}'"));
	                    string deleteSql = $"DELETE FROM [{tableName}] WHERE {whereClause}";
	                    using (var cmd = new SqlCommand(deleteSql, conn, tran))
	                    {
	                        cmd.ExecuteNonQuery();
	                    }
	                }
	
	                // Bulk insert
	                using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran))
	                {
	                    bulk.DestinationTableName = tableName;
	                    bulk.WriteToServer(dataTable);
	                }
	
	                tran.Commit();
	            }
	        }
	    }
	
	    private DataTable GetTableSchema(string tableName)
	    {
	        using (var conn = new SqlConnection(_connectionString))
	        {
	            string sql = @"SELECT COLUMN_NAME, DATA_TYPE 
	                           FROM INFORMATION_SCHEMA.COLUMNS 
	                           WHERE TABLE_NAME = @TableName";
	            using (var cmd = new SqlCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@TableName", tableName);
	                var dt = new DataTable();
	                conn.Open();
	                dt.Load(cmd.ExecuteReader());
	                return dt;
	            }
	        }
	    }
	
	    private string[] GetPrimaryKeys(string tableName)
	    {
	        using (var conn = new SqlConnection(_connectionString))
	        {
	            string sql = @"SELECT COLUMN_NAME 
	                           FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
	                           WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
	                           AND TABLE_NAME = @TableName";
	            using (var cmd = new SqlCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@TableName", tableName);
	                var dt = new DataTable();
	                conn.Open();
	                dt.Load(cmd.ExecuteReader());
	                return dt.AsEnumerable().Select(r => r["COLUMN_NAME"].ToString()).ToArray();
	            }
	        }
	    }
	
	    private Type GetTypeFromSqlType(string sqlType)
	    {
	        switch (sqlType.ToLower())
	        {
	            case "int": return typeof(int);
	            case "bigint": return typeof(long);
	            case "bit": return typeof(bool);
	            case "datetime": return typeof(DateTime);
	            case "decimal": return typeof(decimal);
	            case "float": return typeof(double);
	            case "nvarchar":
	            case "varchar":
	            case "nchar":
	            case "char":
	            case "text":
	                return typeof(string);
	            default: return typeof(string);
	        }
	    }
	}
}
