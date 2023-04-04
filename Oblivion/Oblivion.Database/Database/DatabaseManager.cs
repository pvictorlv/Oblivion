#region

using System;
using System.Threading.Tasks;
using MySqlConnector;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database
{
    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string host, uint port, string user, string pass, string db, uint maxPool)
        {
            var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = port,
                UserID = user,
                Password = pass,
                Database = db,
                MinimumPoolSize = 1,
                MaximumPoolSize = maxPool,
                Pooling = true,
                AllowZeroDateTime = true,
                ConvertZeroDateTime = true,
                DefaultCommandTimeout = 120,
                ConnectionTimeout = 30

            };
            _connectionStr = mySqlConnectionStringBuilder.ToString();
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient databaseClient = new DatabaseConnection(_connectionStr);
                databaseClient.Connect();
                return databaseClient.GetQueryReactor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<IQueryAdapter> GetQueryReactorAsync()
        {
            try
            {
                IDatabaseClient databaseClient = new DatabaseConnection(_connectionStr);
                await databaseClient.ConnectAsync();
                return databaseClient.GetQueryReactor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void Destroy()
        {
        }
    }
}