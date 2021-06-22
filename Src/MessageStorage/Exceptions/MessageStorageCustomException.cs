using System;

namespace MessageStorage.Exceptions
{
    public class MessageStorageCustomException : Exception
    {
        public MessageStorageCustomException(string friendlyMessage, Exception? innerEx = null)
            : base(friendlyMessage, innerEx)
        {
        }
    }
}