using System;

namespace MessageStorage.Exceptions.BaseExceptions
{
    public abstract class MessageStorageCustomException : Exception
    {
        public MessageStorageCustomException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}