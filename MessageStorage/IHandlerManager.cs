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
        private readonly List<Handler> _handlers;
        private readonly object _lockObj;

        public HandlerManager(IEnumerable<Handler> handlers = null)
        {
            _lockObj = new object();
            _handlers = handlers?.ToList().GroupBy(h => h.Name).Select(g => g.First()).ToList()
                     ?? new List<Handler>();
            Handlers = _handlers.AsReadOnly();
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
                throw new HandlerNotFoundException($"Handler Name does not supplied");
            Handler handler = Handlers.FirstOrDefault(h => h.Name == handlerName);
            if (handler == null)
                throw new HandlerNotFoundException($"Handler could not found #{handlerName}");
            return handler;
        }
    }
}