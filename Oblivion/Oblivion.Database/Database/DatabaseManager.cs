#region

using System;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database
{
    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;
        private readonly string _typer;

        public DatabaseManager(string connectionStr, string connType)
        {
            _connectionStr = connectionStr;
            _typer = connType;
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient databaseClient = new DatabaseConnection(_connectionStr, _typer);
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