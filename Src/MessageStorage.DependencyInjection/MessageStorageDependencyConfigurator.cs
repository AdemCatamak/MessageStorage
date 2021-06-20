using System;
using System.Reflection;
using MessageStorage.Containers;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DependencyInjection
{
    public class MessageStorageDependencyConfigurator : IMessageStorageDependencyConfigurator
    {
        private readonly IMessageHandlerContainer _messageHandlerContainer;
        public IServiceCollection ServiceCollection { get; }

        public MessageStorageDependencyConfigurator(IMessageHandlerContainer messageHandlerContainer, IServiceCollection serviceCollection)
        {
            _messageHandlerContainer = messageHandlerContainer;
            ServiceCollection = serviceCollection;
        }

        public void Register<TMessageHandler>(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null) where TMessageHandler : class, IMessageHandler
        {
            _messageHandlerContainer.Register<TMessageHandler>(configureMessageHandlerMetadata);
        }

        public void Register(params Assembly[] assemblies)
        {
            _messageHandlerContainer.Register(assemblies);
        }

        public void Register(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata, params Assembly[] assemblies)
        {
            _messageHandlerContainer.Register(configureMessageHandlerMetadata, assemblies);
        }

        public void Register(Type messageHandlerType, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null)
        {
            _messageHandlerContainer.Register(messageHandlerType, configureMessageHandlerMetadata);
        }
    }
}