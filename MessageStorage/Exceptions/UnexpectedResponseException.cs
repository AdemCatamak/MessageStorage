using System;

namespace MessageStorage.Exceptions
{
    public class UnexpectedResponseException : Exception
    {
        public UnexpectedResponseException(string message) : base(message)
        {
        }
    }
}