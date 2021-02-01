using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class ContextAlreadyHasTransaction : MessageStorageCustomException
    {
        public const string MESSAGE = "Context already has a transaction. You commit or rollback transaction before starting new one.";
        public ContextAlreadyHasTransaction() : base(MESSAGE)
        {
        }
    }
}