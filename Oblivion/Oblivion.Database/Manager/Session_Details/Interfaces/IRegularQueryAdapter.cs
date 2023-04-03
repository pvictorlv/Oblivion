#region

using System;
using System.Data;
using System.Threading.Tasks;

#endregion

namespace Oblivion.Database.Manager.Session_Details.Interfaces
{
    public interface IRegularQueryAdapter : IDisposable
    {
        void AddParameter(string name, object query);

        bool FindsResult();

        int GetInteger();

        DataRow GetRow();

        string GetString();

        DataTable GetTable();

        void RunFastQuery(string query);
        Task RunFastQueryAsync(string query);
        void RunNoLockFastQuery(string query);
        Task RunNoLockFastQueryAsync(string query);

        void SetQuery(string query);
        void SetNoLockQuery(string query);
    }
}