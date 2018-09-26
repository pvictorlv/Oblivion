#region

using Oblivion.Database.Manager.Database.Session_Details;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using MySql.Data.MySqlClient;

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

        public MySqlTransaction GetTransactionMySql()
        {
            return _mySqlConnection.BeginTransaction();
        }

        public bool IsAvailable()
        {
            return _info == null;
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