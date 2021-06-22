using System;
using System.Reflection;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Containers
{
    public interface IMessageHandlerContainer
    {
        void Register<TMessageHandler>(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null) where TMessageHandler : class, IMessageHandler;
        void Register(params Assembly[] assemblies);
        void Register(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata, params Assembly[] assemblies);
        void Register(Type messageHandlerType, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null);
    }
}