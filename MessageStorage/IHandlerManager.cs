using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IHandlerManager
    {
        IReadOnlyCollection<Handler> Handlers { get; }

        IEnumerable<string> GetAvailableHandlerNames(object payload);

        Handler GetHandler(string handlerName);
    }

    public class HandlerManager : IHandlerManager
    {
        public IReadOnlyCollection<Handler> Handlers { get; private set; }

        public HandlerManager(IEnumerable<Handler> handlers = null)
        {
            List<Handler> handlerList = handlers?.GroupBy(h => h.Name)
                                                 .Select(g => g.First())
                                                 .ToList()
                                     ?? new List<Handler>();
            Handlers = handlerList.AsReadOnly();
        }

        public IEnumerable<string> GetAvailableHandlerNames(object payload)
        {
            Type payloadType = payload.GetType();
            IEnumerable<string> availableHandlers = Handlers.Where(h =>
                                                                   {
                                                                       Type handlerType = h.GetType();

                                                                       do
                                                                       {
                                                                           if (handlerType.IsGenericType)
                                                                           {
                                                                               IEnumerable<Type> handlerGenericArgumentTypes = handlerType.GenericTypeArguments;
                                                                               bool isAssignable = handlerGenericArgumentTypes.Any(x => x.IsAssignableFrom(payloadType));
                                                                               return isAssignable;
                                                                           }

                                                                           handlerType = handlerType.BaseType;
                                                                       } while (handlerType != null);

                                                                       return false;
                                                                   }
                                                                  )
                                                            .Select(handler => handler.Name);

            return availableHandlers;
        }


        public Handler GetHandler(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                throw new HandlerNameIsEmptyException();
            Handler handler = Handlers.FirstOrDefault(h => h.Name == handlerName);
            if (handler == null)
                throw new HandlerNotFoundException(handlerName);
            return handler;
        }
    }
}