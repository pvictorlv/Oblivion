#region

using System;
using MySqlConnector;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IDatabaseClient : IDisposable
    {
        void Connect();

        void Disconnect();

        IQueryAdapter GetQueryReactor();

        MySqlCommand CreateNewCommandMySql();
        

        MySqlTransaction GetTransactionMySql();

        bool IsAvailable();

        void ReportDone();
    }
}