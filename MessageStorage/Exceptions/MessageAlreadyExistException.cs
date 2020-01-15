using System;

namespace MessageStorage.Exceptions
{
    public class MessageAlreadyExistException : Exception
    {
        public MessageAlreadyExistException(string identifier) : base($"Message id already registered #{identifier}")
        {
        }
    }
}