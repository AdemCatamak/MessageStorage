using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IHandlerManager
    {
        IReadOnlyCollection<HandlerDescription> HandlerDescriptions { get; }

        IEnumerable<string> GetAvailableHandlerNames(object payload);

        Handler GetHandler(string handlerName);

        bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription) where THandler : Handler;
        bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription, out string errorMessage) where THandler : Handler;
    }

    public class HandlerManager : IHandlerManager
    {
        public IReadOnlyCollection<HandlerDescription> HandlerDescriptions => _handlerDescriptions.AsReadOnly();
        private readonly List<HandlerDescription> _handlerDescriptions;

        public HandlerManager(IEnumerable<HandlerDescription>? handlerDescriptions = null)
        {
            _handlerDescriptions = handlerDescriptions?.Where(h => !string.IsNullOrEmpty(h.HandlerName))
                                                       .GroupBy(h => h.HandlerName)
                                                       .Select(g => g.First())
                                                       .ToList()
                                ?? new List<HandlerDescription>();
        }

        public IEnumerable<string> GetAvailableHandlerNames(object payload)
        {
            Type payloadType = payload.GetType();
            IEnumerable<string> availableHandlers = HandlerDescriptions.Where(h =>
                                                                              {
                                                                                  Type acceptedPayloadTypeByHandler = h.PayloadType;
                                                                                  bool isAssignableFrom = acceptedPayloadTypeByHandler.IsAssignableFrom(payloadType);
                                                                                  return isAssignableFrom;
                                                                              }
                                                                             )
                                                                       .Select(h => h.HandlerName);

            return availableHandlers;
        }

        public Handler GetHandler(string handlerName)
        {
            HandlerDescription? handlerDescription = HandlerDescriptions.FirstOrDefault(h => h.HandlerName == handlerName);
            if (handlerDescription == null)
                throw new HandlerDescriptionNotFoundException(handlerName);

            Handler handler = handlerDescription.HandlerFactoryMethod();
            return handler;
        }

        public bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription) where THandler : Handler
        {
            return TryAddHandler(handlerDescription, out string _);
        }

        public bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription, out string errorMessage) where THandler : Handler
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(handlerDescription.HandlerName))
            {
                errorMessage = HandlerNameIsEmptyException.MESSAGE;
                return false;
            }

            if (_handlerDescriptions.Any(x => x.HandlerName == handlerDescription.HandlerName))
            {
                errorMessage = HandlerAlreadyExistException.MESSAGE;
                return false;
            }

            _handlerDescriptions.Add(handlerDescription);
            return true;
        }
    }
}