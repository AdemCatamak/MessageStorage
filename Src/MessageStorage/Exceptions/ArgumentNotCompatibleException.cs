using System;

namespace MessageStorage.Exceptions
{
    public class ArgumentNotCompatibleException : MessageStorageCustomException
    {
        public ArgumentNotCompatibleException(string actualType, string expectedType)
            : base($"{actualType} is not compatible with {expectedType}")
        {
        }

        public ArgumentNotCompatibleException(Type actualType, Type expectedType)
            : base($"{expectedType.FullName} is expected. Supplied type is {actualType.FullName}")
        {
        }
    }
}