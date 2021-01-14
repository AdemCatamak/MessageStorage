using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.DI.Extension.Exceptions
{
    public class MessageStorageConfigurationBuilderException : MessageStorageCustomException
    {
        public MessageStorageConfigurationBuilderException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}