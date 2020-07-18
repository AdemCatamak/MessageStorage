using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class InsertFailedException : MessageStorageCustomException
    {
        public InsertFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}