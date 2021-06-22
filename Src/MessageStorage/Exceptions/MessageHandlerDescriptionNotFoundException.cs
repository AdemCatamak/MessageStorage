namespace MessageStorage.Exceptions
{
    public class MessageHandlerDescriptionNotFoundException
        : MessageStorageCustomException
    {
        public MessageHandlerDescriptionNotFoundException(string messageHandlerTypeName)
            : base($"Message handler description could not found for {messageHandlerTypeName}")
        {
        }
    }
}