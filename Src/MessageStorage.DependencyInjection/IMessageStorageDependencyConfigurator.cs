using System;
using System.Reflection;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DependencyInjection
{
    public interface IMessageStorageDependencyConfigurator
    {
        IServiceCollection ServiceCollection { get; }

        void Register<TMessageHandler>(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null) where TMessageHandler : class, IMessageHandler;
        void Register(params Assembly[] assemblies);
        void Register(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata, params Assembly[] assemblies);
        void Register(Type messageHandlerType, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null);
    }
}