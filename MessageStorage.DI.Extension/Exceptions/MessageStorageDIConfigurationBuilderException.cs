using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.DI.Extension.Exceptions
{
    public class MessageStorageDIConfigurationBuilderException : MessageStorageCustomException
    {
        public MessageStorageDIConfigurationBuilderException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}