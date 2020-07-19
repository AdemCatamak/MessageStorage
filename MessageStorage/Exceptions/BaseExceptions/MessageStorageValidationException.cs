namespace MessageStorage.Exceptions.BaseExceptions
{
    public abstract class MessageStorageValidationException : MessageStorageCustomException
    {
        protected MessageStorageValidationException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}