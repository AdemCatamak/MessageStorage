namespace MessageStorage.Exceptions;

public class TransactionAlreadyStartedException : BaseMessageStorageException
{
    public TransactionAlreadyStartedException() : base("Message Storage Client already has a transaction")
    {
    }
}