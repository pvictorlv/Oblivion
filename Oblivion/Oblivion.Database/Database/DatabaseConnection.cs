#region

using System.Data;
using Oblivion.Database.Manager.Database.Session_Details;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using MySql.Data.MySqlClient;

#endregion

namespace Oblivion.Database
{
    public class DatabaseConnection : IDatabaseClient
    {
        private MySqlConnection _mysqlConnection;
        private IQueryAdapter _adapter;

        public DatabaseConnection(string connectionStr)
        {
            _mysqlConnection = new MySqlConnection(connectionStr);
            _adapter = new QueryAdapter(this);
        }

        public void Open()
        {
            if (_mysqlConnection.State == ConnectionState.Closed)
                _mysqlConnection.Open();
        }

        public void Close()
        {
            if (_mysqlConnection.State == ConnectionState.Open)
                _mysqlConnection.Close();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_mysqlConnection.State == ConnectionState.Open)
            {
                _mysqlConnection.Close();
            }
            _mysqlConnection.Dispose();
            _mysqlConnection = null;
            _adapter.Dispose();
            _adapter = null;
        }

        public void Connect()
        {
            Open();
        }

        public void Disconnect()
        {
            Close();
        }

        public IQueryAdapter GetQueryReactor() => _adapter;

        public bool IsAvailable() => false;


        public void ReportDone()
        {
            Dispose();
        }


        public MySqlCommand CreateNewCommandMySql() => _mysqlConnection.CreateCommand();

        public MySqlTransaction GetTransactionMySql() => _mysqlConnection.BeginTransaction();
    }
}