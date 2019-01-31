#region

using System;
using MySql.Data.MySqlClient;
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
                DefaultCommandTimeout = 30,
                ConnectionTimeout = 10

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

        public void Destroy()
        {
        }
    }
}