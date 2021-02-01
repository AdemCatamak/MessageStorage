using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class ArgumentNotCompatibleException : MessageStorageCustomException
    {
        public ArgumentNotCompatibleException(string actualType, string expectedType)
            : base($"{actualType} is not compatible with {expectedType}")
        {
        }
    }
}