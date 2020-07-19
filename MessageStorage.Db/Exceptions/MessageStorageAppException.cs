using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class MessageStorageAppException : MessageStorageCustomException
    {
        public MessageStorageAppException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}