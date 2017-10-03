#region

using System;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database
{
    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string connectionStr)
        {
            _connectionStr = connectionStr;
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