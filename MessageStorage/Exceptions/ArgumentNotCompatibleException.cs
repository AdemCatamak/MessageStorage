using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class ArgumentNotCompatibleException : MessageStorageValidationException
    {
        public ArgumentNotCompatibleException(string argumentType, string targetType)
            : this($"{argumentType} is not compatible with {targetType}")
        {
        }

        public ArgumentNotCompatibleException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}