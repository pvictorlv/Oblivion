#region

using System;
using System.Collections.Generic;
using System.Threading;
using Oblivion.Database.Manager.Database.Database_Exceptions;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.Database.Manager.Managers.Database;
using MySql.Data.MySqlClient;

#endregion

namespace Oblivion.Database.Manager.Database
{
    public class DatabaseManager
    {
        public static bool DbEnabled = true;
        private readonly uint _beginClientAmount;
        private readonly uint _maxPoolSize;
        private string _connectionString;
        private List<MySqlClient> _databaseClients;
        private bool _isConnected;
        private DatabaseServer _server;

        public DatabaseManager(uint maxPoolSize, uint clientAmount)
        {
            if (maxPoolSize < clientAmount)
                throw new DatabaseException("The poolsize can not be larger than the client amount!");
            _beginClientAmount = clientAmount;
            _maxPoolSize = maxPoolSize;
        }

        public void Destroy()
        {
            var flag = false;
            try
            {
                Monitor.Enter(this, ref flag);
                _isConnected = false;
                if (_databaseClients == null)
                    return;
                /* TODO CHECK */ foreach (var current in _databaseClients)
                {
                    if (!current.IsAvailable())
                        current.Dispose();
                    current.Disconnect();
                }
                _databaseClients.Clear();
            }
            finally
            {
                if (flag)
                    Monitor.Exit(this);
            }
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public IQueryAdapter GetQueryReactor()
        {
            IDatabaseClient databaseClient = new MySqlClient(this);
            databaseClient.Connect();
            return databaseClient.GetQueryReactor();
        }
        

        public void Init()
        {
            try
            {
                CreateNewConnectionString();
                _databaseClients = new List<MySqlClient>(((int)_maxPoolSize));
            }
            catch (MySqlException ex)
            {
                _isConnected = false;
                throw new Exception($"Could not connect the clients to the database: {ex.Message}");
            }
            _isConnected = true;
        }

        public bool IsConnectedToDatabase() => _isConnected;

        public bool SetServerDetails(string host, uint port, string username, string password, string databaseName)
        {
            bool result;
            try
            {
                _server = new DatabaseServer(host, port, username, password, databaseName);
                result = true;
            }
            catch (DatabaseException)
            {
                _isConnected = false;
                result = false;
            }
            return result;
        }

        private void CreateNewConnectionString()
        {
            var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = _server.GetHost(),
                Port = _server.GetPort(),
                UserID = _server.GetUserName(),
                Password = _server.GetPassword(),
                Database = _server.GetDatabaseName(),
                MinimumPoolSize = _beginClientAmount,
                MaximumPoolSize = _maxPoolSize,
                Pooling = true,
                AllowZeroDateTime = true,
                ConvertZeroDateTime = true,
                DefaultCommandTimeout = 30,
                ConnectionTimeout = 10,
                Logging = false,

            };
            var mySqlConnectionStringBuilder2 = mySqlConnectionStringBuilder;
            SetConnectionString(mySqlConnectionStringBuilder2.ToString());
        }

        private void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}