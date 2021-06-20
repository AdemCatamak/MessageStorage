using System;

namespace MessageStorage.Exceptions
{
    public class MessageHandlerCreationException : Exception
    {
        public MessageHandlerCreationException(string handlerTypeName, Exception? innerException = null)
            : base($"{handlerTypeName} could not initialized", innerException)
        {
        }
    }
}