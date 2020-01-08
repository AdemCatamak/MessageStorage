using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage.HandlerFactorySection
{
    public class HandlerFactory : IHandlerFactory
    {
        public IReadOnlyCollection<IHandler> Handlers => _handlers.AsReadOnly();
        private List<IHandler> _handlers;

        public HandlerFactory(IEnumerable<IHandler> handlers = null)
        {
            _handlers = handlers?.ToList() ?? new List<IHandler>();
        }

        public IEnumerable<string> GetAvailableHandlers(object payload)
        {
            Type payloadType = payload.GetType();
            IEnumerable<string> availableHandlers = _handlers.Where(h =>
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

        public void AddHandler<T>(IHandler handler)
        {
            _handlers.Add(handler);
        }

        public void RemoveHandler(string handlerName)
        {
            _handlers = _handlers.Where(handler => handler.Name == handlerName)
                                 .ToList();
        }


        public IHandler GetHandler(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                throw new HandlerNotFoundException($"Handler type does not supplied");
            return Handlers.FirstOrDefault(h => h.Name == handlerName);
        }
    }
}