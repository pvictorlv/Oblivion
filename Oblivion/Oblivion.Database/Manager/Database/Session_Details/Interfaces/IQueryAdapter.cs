#region

using System;
using System.Data;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IQueryAdapter : IDisposable
    {
        void AddParameter(string name, object query);

        bool FindsResult();

        int GetInteger();

        DataRow GetRow();

        string GetString();

        DataTable GetTable();

        void RunFastQuery(string query);
        void RunNoLockFastQuery(string query);

        void SetQuery(string query);
        void SetNoLockQuery(string query);

        long InsertQuery();

        void RunQuery();
        void RunQuery(string query);
        void RunNoLockQuery(string query);
    }
}