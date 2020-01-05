using System;

namespace MessageStorage.Exceptions
{
    public class HandlerAddOperationException : Exception
    {
        public HandlerAddOperationException(string message = null) : base(message)
        {
        }
    }
}