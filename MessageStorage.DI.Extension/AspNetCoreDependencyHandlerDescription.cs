using System;

namespace MessageStorage.DI.Extension
{
    public class AspNetCoreDependencyHandlerDescription<T> : HandlerDescription
    {
        public AspNetCoreDependencyHandlerDescription(IServiceProvider serviceProvider,
                                                      Func<IServiceProvider, Handler> handlerFactoryMethod)
            : base(() => handlerFactoryMethod.Invoke(serviceProvider))
        {
        }
    }
}