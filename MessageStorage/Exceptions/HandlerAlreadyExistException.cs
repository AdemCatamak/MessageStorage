using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class HandlerAlreadyExistException : MessageStorageCustomException
    {
        public const string MESSAGE = "Handler already exist";

        public HandlerAlreadyExistException() : base(MESSAGE)
        {
        }
    }
}