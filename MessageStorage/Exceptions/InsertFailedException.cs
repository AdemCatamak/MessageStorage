using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class InsertFailedException : MessageStorageCustomException
    {
        public InsertFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}