namespace MessageStorage.Exceptions.BaseExceptions
{
    public abstract class MessageStorageNotFoundException : MessageStorageCustomException
    {
        protected MessageStorageNotFoundException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}