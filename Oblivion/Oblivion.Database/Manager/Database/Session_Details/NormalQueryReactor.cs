#region

using Oblivion.Database.Manager.Database.Database_Exceptions;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details
{
    public class NormalQueryReactor : QueryAdapter, IQueryAdapter
    {
        public NormalQueryReactor(IDatabaseClient client) : base(client)
        {
            CommandMySql = client.CreateNewCommandMySql();
        }

        /* public void Dispose()
         {
             switch (DatabaseManager.DatabaseConnectionType.ToLower())
             {
                 case "firebird":
                     CommandFireBird.Dispose();
                     Client.ReportDone();
                     break;
 
                 case "ingres":
                 case "ingress":
                     CommandIngress.Dispose();
                     Client.ReportDone();
                     break;
 
                 case "pgsql":
                     CommandPgSql.Dispose();
                     Client.ReportDone();
                     break;
 
                 default: // mySql
                     CommandMySql.Dispose();
                     Client.ReportDone();
                     break;
             }
         }*/

        public void DoCommit()
        {
            new TransactionException("Can't use rollback on a non-transactional Query reactor");
        }

        public void DoRollBack()
        {
            new TransactionException("Can't use rollback on a non-transactional Query reactor");
        }
    }
}