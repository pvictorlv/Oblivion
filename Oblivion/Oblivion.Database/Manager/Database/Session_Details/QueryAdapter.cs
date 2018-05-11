#region

using System;
using System.Data;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.Database.Manager.Session_Details.Interfaces;
using MySql.Data.MySqlClient;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details
{
    public class QueryAdapter : IRegularQueryAdapter
    {
        protected IDatabaseClient Client;
        protected MySqlCommand CommandMySql;

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CommandMySql?.Dispose();
            CommandMySql = null;

            Client?.ReportDone();
            
            Client = null;
            GC.SuppressFinalize(this);
        }

        public QueryAdapter(IDatabaseClient client) => Client = client;

        private static bool DbEnabled => DatabaseManager.DbEnabled;

        public void AddParameter(string name, byte[] data)
        {
            CommandMySql.Parameters.Add(new MySqlParameter(name, MySqlDbType.Blob, data.Length));
        }

        public void AddParameter(string parameterName, object val)
        {
            try
            {
                CommandMySql.Parameters.AddWithValue(parameterName, val);
            }
            catch (Exception e)
            {
                Writer.Writer.LogQueryError(e, CommandMySql?.CommandText);
            }
        }


        public bool FindsResult()
        {
            if (!DbEnabled)
                return false;
            var hasRows = false;

            try
            {
                using (var reader = CommandMySql.ExecuteReader())
                    hasRows = reader.HasRows;
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
            }


            return hasRows;
        }

        public int GetInteger()
        {
            if (!DbEnabled)
                return 0;

            var result = 0;
            try
            {
                var obj2 = CommandMySql.ExecuteScalar();
                if (obj2 != null)
                    int.TryParse(obj2.ToString(), out result);
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
            }
            return result;
        }

        public DataRow GetRow()
        {
            if (!DbEnabled)
                return null;

            DataRow row = null;

            try
            {
                var dataSet = new DataSet();
                using (var adapter = new MySqlDataAdapter(CommandMySql))
                    adapter.Fill(dataSet);
                if ((dataSet.Tables.Count > 0) && (dataSet.Tables[0].Rows.Count == 1))
                    row = dataSet.Tables[0].Rows[0];
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
            }

            return row;
        }

        public string GetString()
        {
            if (!DbEnabled)
                return string.Empty;

            var str = string.Empty;


            try
            {
                var obj2 = CommandMySql.ExecuteScalar();
                if (obj2 != null)
                    str = obj2.ToString();
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
                //  throw exception;
            }

            return str;
        }

        public DataTable GetTable()
        {
            var dataTable = new DataTable();
            if (!DbEnabled)
                return dataTable;


            try
            {
                using (var adapter = new MySqlDataAdapter(CommandMySql))
                    adapter.Fill(dataTable);
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
            }
            return dataTable;
        }

        public long InsertQuery()
        {
            if (!DbEnabled)
                return 0L;
            var lastInsertedId = 0L;

            try
            {
                CommandMySql.ExecuteScalar();
                lastInsertedId = CommandMySql.LastInsertedId;
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql.CommandText);
            }
            return lastInsertedId;
        }

        public void RunFastQuery(string query)
        {
            if (!DbEnabled)
                return;
            SetQuery(query);
            RunQuery();
        }

        public void RunQuery()
        {
            if (!DbEnabled)
                return;

            try
            {
//                Writer.Writer.WriteLine(CommandMySql.CommandText);
                CommandMySql.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);
            }
        }

        public void SetQuery(string query)
        {
            try
            {
//                Writer.Writer.WriteLine(query);
                CommandMySql.Parameters.Clear();
                CommandMySql.CommandText = query;
            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);

            }
        }
   
        public void RunQuery(string query)
        {
            try
            {
//                Writer.Writer.WriteLine(query);
//                CommandMySql.Parameters.Clear();
                CommandMySql.CommandText = query;
                CommandMySql.ExecuteNonQuery();
                CommandMySql.Parameters.Clear();

            }
            catch (Exception exception)
            {
                Writer.Writer.LogQueryError(exception, CommandMySql?.CommandText);

            }
        }
    }
}