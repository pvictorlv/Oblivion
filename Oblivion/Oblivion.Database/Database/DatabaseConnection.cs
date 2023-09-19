#region

using System.Data;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using MySqlConnector;

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
            _adapter = new NormalQueryReactor(this);
        }


        public MySqlConnection GetConnection()
        {
            return _mysqlConnection;
        }

        public void Open()
        {
            if (_mysqlConnection.State == ConnectionState.Closed)
                _mysqlConnection.Open();
        }
        public async Task OpenAsync()
        {
            if (_mysqlConnection.State == ConnectionState.Closed)
                await _mysqlConnection.OpenAsync();
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

        public async Task ConnectAsync()
        {
            await OpenAsync();
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