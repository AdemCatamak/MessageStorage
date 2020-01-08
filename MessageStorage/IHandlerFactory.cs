using System.Collections.Generic;

namespace MessageStorage
{
    public interface IHandlerFactory
    {
        IReadOnlyCollection<IHandler> Handlers { get; }
        
        IEnumerable<string> GetAvailableHandlers(object payload);

        void AddHandler<T>(IHandler handler);
        void RemoveHandler(string handlerName);
        IHandler GetHandler(string handlerName);
    }
}