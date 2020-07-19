using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Db.Exceptions
{
    public class DbColumnMapException : MessageStorageCustomException
    {
        public DbColumnMapException(string columnName, string typeName) : base($"{columnName} could not fetch as {typeName}")
        {
        }
    }
}