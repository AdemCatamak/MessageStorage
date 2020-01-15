using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IHandlerManager
    {
        IReadOnlyCollection<Handler> Handlers { get; }

        IEnumerable<string> GetAvailableHandlers(object payload);

        void AddHandler<T>(Handler handler);
        Handler GetHandler(string handlerName);
    }

    public class HandlerManager : IHandlerManager
    {
        public IReadOnlyCollection<Handler> Handlers { get; private set; }
        private List<Handler> _handlers;
        private readonly object _lockObj;

        public HandlerManager(IEnumerable<Handler> handlers = null)
        {
            _lockObj = new object();
            _handlers = handlers?.ToList().GroupBy(h => h.Name).Select(g => g.First()).ToList()
                     ?? new List<Handler>();
            Handlers = _handlers.AsReadOnly();
        }

        public IEnumerable<string> GetAvailableHandlers(object payload)
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
                                                                   })
                                                            .Select(handler => handler.Name);

            return availableHandlers;
        }

        public void AddHandler<T>(Handler handler)
        {
            lock (_lockObj)
            {
                if (_handlers.Any(h => h.Name == handler.Name))
                {
                    throw new HandlerAlreadyExist(handler.Name);
                }

                _handlers.Add(handler);

                Handlers = _handlers.AsReadOnly();
            }
        }


        public Handler GetHandler(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                throw new HandlerNotFoundException($"Handler type does not supplied");
            return Handlers.FirstOrDefault(h => h.Name == handlerName);
        }
    }
}