using System;

namespace MessageStorage.Exceptions
{
    public class ArgumentNotCompatibleException : Exception
    {
        public ArgumentNotCompatibleException(string message) : base(message)
        {
        }

        public ArgumentNotCompatibleException(Type expectedType, Type actualType)
            : base($"Expected Type: {expectedType.Name} - Actual Type:{actualType.Name}")
        {
        }
    }
}