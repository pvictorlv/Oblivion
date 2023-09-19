#region

using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using MySqlConnector;

#endregion

namespace Oblivion.Database.Manager.Database
{
    public class MySqlClient : IDatabaseClient
    {
        private readonly MySqlConnection _mySqlConnection;
        private IQueryAdapter _info;

        public MySqlClient(DatabaseManager dbManager)
        {

            _mySqlConnection = new MySqlConnection(dbManager.GetConnectionString());
        }

        public void Connect()
        {
            _mySqlConnection.Open();
        }
        
        public Task ConnectAsync()
        {
            return _mySqlConnection.OpenAsync();
        }

        public void Disconnect()
        {
            try
            {
                _mySqlConnection.Close();
            }
            catch
            {
            }
        }
        public Task DisconnectAsync()
        {
            try
            {
                return _mySqlConnection.CloseAsync();
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _info = null;
            Disconnect();
        }

        public MySqlCommand GetNewCommandMySql()
        {
            return _mySqlConnection.CreateCommand();
        }


        public IQueryAdapter GetQueryReactor()
        {
            return _info;
        }

        public MySqlConnection GetConnection()
        {
            return _mySqlConnection;
        }

        public MySqlTransaction GetTransactionMySql()
        {
            return _mySqlConnection.BeginTransaction();
        }

        public bool IsAvailable()
        {
            return _info == null;
        }

        public void Prepare()
        {
            _info = new NormalQueryReactor(this);
        }

        public void ReportDone()
        {
            Dispose();
        }

        public MySqlCommand CreateNewCommandMySql()
        {
            return _mySqlConnection.CreateCommand();
        }
    }
}