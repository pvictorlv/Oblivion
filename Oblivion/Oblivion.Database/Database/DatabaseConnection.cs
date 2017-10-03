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
        private readonly MySqlConnection _mysqlConnection;
        private readonly IQueryAdapter _adapter;

        public DatabaseConnection(string connectionStr)
        {
            _mysqlConnection = new MySqlConnection(connectionStr);
            _adapter = new NormalQueryReactor(this);
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

        public void Dispose()
        {
            if (_mysqlConnection.State == ConnectionState.Open)
            {
                _mysqlConnection.Close();
            }
            _mysqlConnection.Dispose();
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