#region

using System.Threading.Tasks;
using Oblivion.Database.Manager.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IQueryAdapter : IRegularQueryAdapter
    {
        void DoCommit();

        void DoRollBack();

        long InsertQuery();
        Task<long> InsertQueryAsync();

        void RunQuery();
        Task RunQueryAsync();
        void RunQuery(string query);
        void RunNoLockQuery(string query);
    }
}