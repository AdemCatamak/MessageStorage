using System;

namespace MessageStorage
{
    public class HandlerDescription<THandler>
        : HandlerDescription
        where THandler : Handler
    {
        public HandlerDescription(Func<THandler> handlerFactoryMethod)
            : base(handlerFactoryMethod)
        {
        }
    }

    public abstract class HandlerDescription
    {
        public string HandlerName { get; protected set; }
        public Type PayloadType { get; protected set; }

        public Func<Handler> HandlerFactoryMethod { get; private set; }

        protected HandlerDescription(Func<Handler> handlerFactoryMethod)
        {
            Handler handler = handlerFactoryMethod.Invoke();

            HandlerName = handler.Name;
            PayloadType = handler.PayloadType();
            HandlerFactoryMethod = handlerFactoryMethod;
        }

        public virtual Handler GetHandler()
        {
            var handler = HandlerFactoryMethod.Invoke();
            return handler;
        }
    }
}