#region

using System;

#endregion

namespace Oblivion.Database.Manager.Database.Database_Exceptions
{
    public class TransactionException : Exception
    {
        public TransactionException(string message) : base(message)
        {
        }
    }
}