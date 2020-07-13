using System.Reflection;
using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class InsertFailedException : MessageStorageCustomException
    {
        public InsertFailedException(MemberInfo entityType) : base($"{entityType.Name} could not insert")
        {
        }

        public InsertFailedException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}