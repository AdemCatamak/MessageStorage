namespace MessageStorage.Exceptions.BaseExceptions
{
    public class MessageStorageNotFoundException : MessageStorageCustomException
    {
        public MessageStorageNotFoundException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}