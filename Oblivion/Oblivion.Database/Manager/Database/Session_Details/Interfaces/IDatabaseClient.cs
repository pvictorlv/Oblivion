#region

using System;
using FirebirdSql.Data.FirebirdClient;
using Ingres.Client;
using MySql.Data.MySqlClient;
using Npgsql;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IDatabaseClient : IDisposable
    {
        void Connect();

        void Disconnect();

        IQueryAdapter GetQueryReactor();

        MySqlCommand CreateNewCommandMySql();

        FbCommand CreateNewCommandFireBird();

        IngresCommand CreateNewCommandIngress();

        IngresTransaction GetTransactionIngress();

        NpgsqlCommand CreateNewCommandPgSql();

        NpgsqlTransaction GetTransactionPgSql();

        FbTransaction GetTransactionFireBird();

        MySqlTransaction GetTransactionMySql();

        bool IsAvailable();

        void Prepare();

        void ReportDone();
    }
}