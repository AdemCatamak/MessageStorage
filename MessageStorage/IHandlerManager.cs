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

        bool TryAddHandler(Handler handler);
        bool TryAddHandler(Handler handler, out string errorMessage);
    }

    public class HandlerManager : IHandlerManager
    {
        public IReadOnlyCollection<Handler> Handlers => _handlers.AsReadOnly();
        private readonly List<Handler> _handlers;

        public HandlerManager(IEnumerable<Handler>? handlers = null)
        {
            _handlers = handlers?.Where(h => !string.IsNullOrEmpty(h.Name))
                                 .GroupBy(h => h.Name)
                                 .Select(g => g.First())
                                 .ToList()
                     ?? new List<Handler>();
        }

        public IEnumerable<string> GetAvailableHandlerNames(object payload)
        {
            Type payloadType = payload.GetType();
            IEnumerable<string> availableHandlers = Handlers.Where(h =>
                                                                   {
                                                                       Type? handlerType = h.GetType();

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
            Handler? handler = Handlers.FirstOrDefault(h => h.Name == handlerName);
            if (handler == null)
                throw new HandlerNotFoundException(handlerName);
            return handler;
        }

        public bool TryAddHandler(Handler handler)
        {
            return TryAddHandler(handler, out string _);
        }

        public bool TryAddHandler(Handler handler, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(handler.Name))
            {
                errorMessage = HandlerNameIsEmptyException.MESSAGE;
                return false;
            }

            if (_handlers.Any(x => x.Name == handler.Name))
            {
                errorMessage = HandlerAlreadyExistException.MESSAGE;
                return false;
            }

            _handlers.Add(handler);
            return true;
        }
    }
}