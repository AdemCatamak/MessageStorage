using System;

namespace MessageStorage.Exceptions
{
    public class ArgumentNotCompatibleException : Exception
    {
        public ArgumentNotCompatibleException(string message) : base(message)
        {
        }
    }
}