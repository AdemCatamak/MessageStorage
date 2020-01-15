using System;

namespace MessageStorage.Db.Exceptions
{
    public class DbGetOperationException : Exception
    {
        public DbGetOperationException(string message = null) : base(message)
        {
        }
    }
}