using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class HandlerNameIsEmptyException : MessageStorageCustomException
    {
        public const string MESSAGE = "Handler name should not be null or empty";

        public HandlerNameIsEmptyException() : base(MESSAGE)
        {
        }
    }
}