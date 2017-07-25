#region

using System;
using Oblivion.Database.Manager.Session_Details.Interfaces;

#endregion

namespace Oblivion.Database.Manager.Database.Session_Details.Interfaces
{
    public interface IQueryAdapter : IRegularQueryAdapter, IDisposable
    {
        void DoCommit();

        void DoRollBack();

        long InsertQuery();

        void RunQuery();
    }
}