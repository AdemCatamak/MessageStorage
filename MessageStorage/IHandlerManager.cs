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

        bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription) where THandler : Handler, new();
        bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription, out string errorMessage) where THandler : Handler, new();
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
                                                                                  var parameterType = h.PayloadType;
                                                                                  bool isAssignableFrom = parameterType.IsAssignableFrom(payloadType);
                                                                                  return isAssignableFrom;
                                                                              }
                                                                             )
                                                                       .Select(h => h.HandlerName);

            return availableHandlers;
        }

        public Handler GetHandler(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                throw new HandlerNameIsEmptyException();

            HandlerDescription? handlerDescription = HandlerDescriptions.FirstOrDefault(h => h.HandlerName == handlerName);
            if (handlerDescription == null)
                throw new HandlerDescriptionNotFoundException(handlerName);

            return handlerDescription.HandlerFactoryMethod();
        }

        public bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription) where THandler : Handler, new()
        {
            return TryAddHandler(handlerDescription, out string _);
        }

        public bool TryAddHandler<THandler>(HandlerDescription<THandler> handlerDescription, out string errorMessage) where THandler : Handler, new()
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