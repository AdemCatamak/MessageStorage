using MessageStorage.Exceptions.BaseExceptions;

namespace MessageStorage.Exceptions
{
    public class HandlerDescriptionNotFoundException : MessageStorageCustomException
    {
        public HandlerDescriptionNotFoundException(string handlerName) : base($"Handler description could not found #{handlerName}")
        {
        }
    }
}