using System;

namespace MessageStorage.Exceptions
{
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(string message = null, Exception innerEx = null) : base(message, innerEx)
        {
        }
    }
}