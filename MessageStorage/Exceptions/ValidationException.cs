using System;

namespace MessageStorage.Exceptions
{
    public abstract class ValidationException : Exception
    {
        protected ValidationException(string message, Exception innerEx = null) : base(message, innerEx)
        {
        }
    }

    public class ConnectionStringValidationException : ValidationException
    {
        public ConnectionStringValidationException(string message, Exception innerEx = null) : base(message, innerEx)
        {
        }
    }
}