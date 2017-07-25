#region

using Oblivion.Database.Manager.Database.Database_Exceptions;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Ingres.Client;
using MySql.Data.MySqlClient;
using Npgsql;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details
{
    public class TransactionQueryReactor : QueryAdapter, IQueryAdapter
    {
        private bool _finishedTransaction;
        private MySqlTransaction _transactionmysql;
        private NpgsqlTransaction _transactionpgsql;
        private FbTransaction _transactionfirebird;
        private IngresTransaction _transactioningress;

        public TransactionQueryReactor(IDatabaseClient client) : base(client)
        {
            InitTransaction();
        }

        public void Dispose()
        {
            if (!_finishedTransaction)
                throw new TransactionException(
                    "The transaction needs to be completed by commit() or rollback() before you can dispose this item.");
            switch (DatabaseManager.DatabaseConnectionType.ToLower())
            {
                case "firebird":
                    CommandFireBird.Dispose();
                    break;

                case "ingres":
                case "ingress":
                    CommandIngress.Dispose();
                    break;

                case "pgsql":
                    CommandPgSql.Dispose();
                    break;

                default: // mySql
                    CommandMySql.Dispose();
                    break;
            }
            Client.ReportDone();
        }

        public void DoCommit()
        {
            try
            {
                switch (DatabaseManager.DatabaseConnectionType.ToLower())
                {
                    case "pgsql":
                        _transactionpgsql.Commit();
                        break;

                    case "ingress":
                    case "ingres":
                        _transactioningress.Commit();
                        break;

                    case "firebird":
                        _transactionfirebird.Commit();
                        break;

                    default: // mySql
                        _transactionmysql.Commit();
                        break;
                }
                _finishedTransaction = true;
            }
            catch (MySqlException ex)
            {
                throw new TransactionException(ex.Message);
            }
        }

        public void DoRollBack()
        {
            switch (DatabaseManager.DatabaseConnectionType.ToLower())
            {
                case "firebird":
                case "FireBird":
                    try
                    {
                        _transactionfirebird.Rollback();
                        _finishedTransaction = true;
                    }
                    catch (FbException ex)
                    {
                        throw new TransactionException(ex.Message);
                    }
                    break;

                case "ingres":
                case "ingress":
                    try
                    {
                        _transactioningress.Rollback();
                        _finishedTransaction = true;
                    }
                    catch (IngresException ex)
                    {
                        throw new TransactionException(ex.Message);
                    }
                    break;

                case "pgsql":
                    try
                    {
                        _transactionpgsql.Rollback();
                        _finishedTransaction = true;
                    }
                    catch (NpgsqlException ex)
                    {
                        throw new TransactionException(ex.Message);
                    }
                    break;

                default:
                    try
                    {
                        _transactionmysql.Rollback();
                        _finishedTransaction = true;
                    }
                    catch (MySqlException ex)
                    {
                        throw new TransactionException(ex.Message);
                    }
                    break;
            }
        }

        public bool GetAutoCommit()
        {
            return false;
        }

        private void InitTransaction()
        {
            switch (DatabaseManager.DatabaseConnectionType.ToLower())
            {
                case "firebird":
                    CommandFireBird = Client.CreateNewCommandFireBird();
                    _transactionfirebird = Client.GetTransactionFireBird();
                    CommandFireBird.Transaction = _transactionfirebird;
                    CommandFireBird.Connection = _transactionfirebird.Connection;
                    break;

                case "ingres":
                case "ingress":
                    CommandIngress = Client.CreateNewCommandIngress();
                    _transactioningress = Client.GetTransactionIngress();
                    CommandIngress.Transaction = _transactioningress;
                    break;

                case "pgsql":
                    CommandPgSql = Client.CreateNewCommandPgSql();
                    _transactionpgsql = Client.GetTransactionPgSql();
                    CommandPgSql.Transaction = _transactionpgsql;
                    CommandPgSql.Connection = _transactionpgsql.Connection;
                    break;

                default:
                    CommandMySql = Client.CreateNewCommandMySql();
                    _transactionmysql = Client.GetTransactionMySql();
                    CommandMySql.Transaction = _transactionmysql;
                    CommandMySql.Connection = _transactionmysql.Connection;
                    break;
            }

            _finishedTransaction = false;
        }
    }
}