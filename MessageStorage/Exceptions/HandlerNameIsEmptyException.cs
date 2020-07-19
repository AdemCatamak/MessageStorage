using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class HandlerNameIsEmptyException : MessageStorageValidationException
    {
        public HandlerNameIsEmptyException() : base("Handler name should not be null")
        {
        }
    }
}