#region

using System;

#endregion

namespace Oblivion.Database.Manager.Database.Database_Exceptions
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }
    }
}