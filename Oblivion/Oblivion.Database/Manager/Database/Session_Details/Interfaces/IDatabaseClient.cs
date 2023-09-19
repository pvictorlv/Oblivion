#region

using System;
using System.Threading.Tasks;
using MySqlConnector;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IDatabaseClient : IDisposable
    {
        void Connect();
        Task ConnectAsync();

        void Disconnect();

        IQueryAdapter GetQueryReactor();

        MySqlCommand CreateNewCommandMySql();
        MySqlConnection GetConnection();
        

        MySqlTransaction GetTransactionMySql();

        bool IsAvailable();

        void ReportDone();
    }
}