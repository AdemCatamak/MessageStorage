using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class UpdateFailedException : MessageStorageCustomException
    {
        public UpdateFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}