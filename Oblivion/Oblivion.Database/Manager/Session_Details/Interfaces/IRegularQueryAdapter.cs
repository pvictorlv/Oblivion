#region

using System.Data;

#endregion

namespace Oblivion.Database.Manager.Session_Details.Interfaces
{
    public interface IRegularQueryAdapter
    {
        void AddParameter(string name, object query);

        bool FindsResult();

        int GetInteger();

        DataRow GetRow();

        string GetString();

        DataTable GetTable();

        void RunFastQuery(string query);

        void SetQuery(string query);
    }
}