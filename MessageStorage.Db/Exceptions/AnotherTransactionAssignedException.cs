using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class AnotherTransactionAssignedException : MessageStorageCustomException
    {
        public AnotherTransactionAssignedException()
            : base($"There is already assigned transaction. Please use RemoveTransaction before assignment of new transaction")
        {
        }
    }
}