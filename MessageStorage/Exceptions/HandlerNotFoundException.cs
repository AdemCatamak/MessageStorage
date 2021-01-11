using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class HandlerNotFoundException : MessageStorageCustomException
    {
        public HandlerNotFoundException(string handlerName) : base($"Handler could not found #{handlerName}")
        {
        }
    }
}