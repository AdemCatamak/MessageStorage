using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage.HandlerFactorySection
{
    public class HandlerFactory : IHandlerFactory
    {
        public IReadOnlyCollection<Handler> Handlers => _handlers.AsReadOnly();
        private List<Handler> _handlers;

        public HandlerFactory(IEnumerable<Handler> handlers = null)
        {
            _handlers = handlers?.ToList() ?? new List<Handler>();
        }

        public IEnumerable<string> GetAvailableHandlers(object payload)
        {
            Type payloadType = payload.GetType();
            return _handlers.Where(h =>
                                   {
                                       Type t = h.GetType();

                                       do
                                       {
                                           if (t.IsGenericType && t.GenericTypeArguments.Contains(payloadType))
                                               return true;
                                           t = t.BaseType;
                                       } while (t != null);

                                       return false;
                                   })
                            .Select(handler => handler.Name);
        }

        public void AddHandler<T>(Handler handler)
        {
            _handlers.Add(handler);
        }

        public void RemoveHandler(string handlerName)
        {
            _handlers = _handlers.Where(handler => handler.Name == handlerName)
                                 .ToList();
        }

        
        /// <summary>
        /// If handler could not found with specified name, result return null
        /// </summary>
        /// <param name="handlerName"></param>
        /// <returns>Null or Handler</returns>
        public Handler GetHandler(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                throw new HandlerNotFoundException($"Handler type does not supplied");
            return Handlers.FirstOrDefault(h => h.Name == handlerName);
        }
    }
}