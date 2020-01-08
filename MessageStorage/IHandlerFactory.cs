using System.Collections.Generic;

namespace MessageStorage
{
    public interface IHandlerFactory
    {
        IReadOnlyCollection<Handler> Handlers { get; }
        
        IEnumerable<string> GetAvailableHandlers(object payload);

        void AddHandler<T>(Handler handler);
        void RemoveHandler(string handlerName);
        Handler GetHandler(string handlerName);
    }
}