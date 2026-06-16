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
    public class SQL_GBL_Get_Max_ID
    {
        private readonly SqlConnection _connection;

        public SQL_GBL_Get_Max_ID(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }
		
		

		
		
		
		public int Get_Max_ID(SessionInfo si, string table, string colName)
{
    int nextId = 1; // Default to 1 if table is empty
    string sql = $"SELECT MAX({colName}) FROM {table}";

    try
    {
        using (SqlCommand command = new SqlCommand(sql, _connection))
        {
            object result = command.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                // Convert.ToInt32 handles different SQL numeric types (decimal, bigint, etc.)
                nextId = Convert.ToInt32(result) + 1;
            }
        }
    }
    catch (Exception ex)
    {
        BRApi.ErrorLog.LogMessage(si, $"Error calculating next ID for {table}: {ex.Message}");
        throw; 
    }

    return nextId;
}

//        public int Get_Max_ID(SessionInfo si, string table, string cols)
//        {
//            var maxId = 0;

//            var sql = $"SELECT MAX({cols}) as MAX FROM {table}";
//BRApi.ErrorLog.LogMessage(si, sql);
//            using (SqlCommand command = new SqlCommand(sql, _connection))
//            {
//                command.CommandType = CommandType.Text;

//                using (SqlDataReader reader = command.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        maxId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
//                    }
//                }
//            }

//            return maxId + 1;
//        }
    }
}