#region

using System;
using Oblivion.Database.Manager.Database.Session_Details;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Ingres.Client;
using MySql.Data.MySqlClient;
using Npgsql;

#endregion

namespace Oblivion.Database.Manager.Database
{
    public class MySqlClient : IDatabaseClient, IDisposable
    {
        private readonly MySqlConnection _mySqlConnection;
        private readonly FbConnection _fireBirdConnection;
        private readonly IngresConnection _inGressConnection;
        private readonly NpgsqlConnection _pgSqlConnection;
        private readonly DatabaseManager _dbManager;
        private IQueryAdapter _info;

        public MySqlClient(DatabaseManager dbManager)
        {
            _dbManager = dbManager;

            switch (DatabaseManager.DatabaseConnectionType.ToLower())
            {
                case "pgsql":
                    _pgSqlConnection = new NpgsqlConnection(dbManager.GetConnectionString());
                    break;

                case "ingress":
                case "ingres":
                    _inGressConnection = new IngresConnection(dbManager.GetConnectionString());
                    break;

                case "firebird":
                    _fireBirdConnection = new FbConnection(dbManager.GetConnectionString());
                    break;

                default: // mySql
                    _mySqlConnection = new MySqlConnection(dbManager.GetConnectionString());
                    break;
            }
        }

        public void Connect()
        {
            switch (DatabaseManager.DatabaseConnectionType.ToLower())
            {
                case "pgsql":
                    _pgSqlConnection.Open();
                    break;

                case "ingress":
                case "ingres":
                    _inGressConnection.Open();
                    break;

                case "firebird":
                    _fireBirdConnection.Open();
                    break;

                default: // mySql
                    _mySqlConnection.Open();
                    break;
            }
        }

        public void Disconnect()
        {
            try
            {
                switch (DatabaseManager.DatabaseConnectionType.ToLower())
                {
                    case "pgsql":
                        _pgSqlConnection.Close();
                        break;

                    case "ingress":
                    case "ingres":
                        _inGressConnection.Close();
                        break;

                    case "firebird":
                        _fireBirdConnection.Close();
                        break;

                    default: // mySql
                        _mySqlConnection.Close();
                        break;
                }
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

        public FbCommand GetNewCommandFireBird()
        {
            return _fireBirdConnection.CreateCommand();
        }

        public IngresCommand GetNewCommandIngress()
        {
            return _inGressConnection.CreateCommand();
        }

        public NpgsqlCommand GetNewCommandPgSql()
        {
            return _pgSqlConnection.CreateCommand();
        }

        public IQueryAdapter GetQueryReactor()
        {
            return _info;
        }

        public MySqlTransaction GetTransactionMySql()
        {
            return _mySqlConnection.BeginTransaction();
        }

        public NpgsqlTransaction GetTransactionPgSql()
        {
            return _pgSqlConnection.BeginTransaction();
        }

        public IngresTransaction GetTransactionIngress()
        {
            return _inGressConnection.BeginTransaction();
        }

        public FbTransaction GetTransactionFireBird()
        {
            return _fireBirdConnection.BeginTransaction();
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

        public FbCommand CreateNewCommandFireBird()
        {
            return _fireBirdConnection.CreateCommand();
        }

        public IngresCommand CreateNewCommandIngress()
        {
            return _inGressConnection.CreateCommand();
        }

        public NpgsqlCommand CreateNewCommandPgSql()
        {
            return _pgSqlConnection.CreateCommand();
        }
    }
}