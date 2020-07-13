using System;
using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class UpdateFailedException : MessageStorageCustomException
    {
        public UpdateFailedException(Type entityType) : base($"{entityType.Name} could not update")
        {
        }

        public UpdateFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}