#region

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
            IDatabaseClient databaseClient = new DatabaseConnection(_connectionStr, _typer);
            databaseClient.Connect();
            databaseClient.Prepare();
            return databaseClient.GetQueryReactor();
        }

        public void Destroy()
        {
        }
    }
}