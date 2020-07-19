using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class UpdateFailedException : MessageStorageCustomException
    {
        public UpdateFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}