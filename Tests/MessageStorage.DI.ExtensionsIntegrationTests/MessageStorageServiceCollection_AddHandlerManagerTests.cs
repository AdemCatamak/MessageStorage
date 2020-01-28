using System;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MessageStorage.DI.ExtensionsIntegrationTests
{
    public class MessageStorageServiceCollection_AddHandlerManagerTests : IDisposable
    {
        private readonly MessageStorageServiceCollection _sut;
        private readonly IServiceCollection _serviceCollection;

        public MessageStorageServiceCollection_AddHandlerManagerTests()
        {
            _serviceCollection = new ServiceCollection();
            _sut = new MessageStorageServiceCollection(_serviceCollection);
        }

        [Fact]
        public void WhenAddHandlerManagerMethodExecuted__IHandlerManager_and_IHandlerManagerConcrete_ShouldBeInjected()
        {
            _sut.AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton);

            using ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

            Assert.NotNull(serviceProvider.GetService<IHandlerManager>());
            Assert.NotNull(serviceProvider.GetService<HandlerManager>());
        }

        public void Dispose()
        {
            _serviceCollection?.Clear();
        }
    }
}