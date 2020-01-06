using System.Collections.Generic;

namespace MessageStorage
{
    public interface IHandlerFactory
    {
        IReadOnlyCollection<Handler> Handlers { get; }

        /// <summary>
        /// Take payload type and return all matching handler's name
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        IEnumerable<string> GetAvailableHandlers(object payload);

        void AddHandler<T>(Handler handler);
        void RemoveHandler(string handlerName);
        Handler GetHandler(string handlerName);
    }
}