using System;

namespace MessageStorage.Exceptions
{
    public class HandlerRemoveOperationException : Exception
    {
        public HandlerRemoveOperationException(string message = null) : base(message)
        {
        }
    }
}