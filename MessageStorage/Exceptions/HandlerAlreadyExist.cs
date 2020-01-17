using System;

namespace MessageStorage.Exceptions
{
    public class HandlerAlreadyExist : Exception
    {
        public HandlerAlreadyExist(string handlerName) : base($"Handler already exist #{handlerName}")
        {
        }
    }
}