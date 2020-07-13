namespace MessageStorage.Exceptions.BaseExceptions
{
    public abstract class MessageStorageValidationException : MessageStorageCustomException
    {
        public MessageStorageValidationException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}