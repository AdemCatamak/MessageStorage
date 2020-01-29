using System;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests
{
    public class InMemoryMessageStorageDIExtensions_AddInMemoryMessageStorageTests
    {
        private readonly MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public InMemoryMessageStorageDIExtensions_AddInMemoryMessageStorageTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddInMemoryMessageStorageMethodExecute__IMessageStorageClient_and_IHandlerManager_ShouldBeInjected()
        {
            _sut.AddInMemoryMessageStorage();

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor =>
                                                                                                    descriptor.ServiceType == typeof(IHandlerManager)
                                                                                                 && descriptor.ImplementationType == typeof(HandlerManager)
                                                                                                 && descriptor.Lifetime == ServiceLifetime.Singleton)), Times.Once);

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor =>
                                                                                                    descriptor.ServiceType == typeof(IMessageStorageClient)
                                                                                                 && descriptor.ImplementationFactory.GetType() == typeof(Func<IServiceProvider, IMessageStorageClient>)
                                                                                                 && descriptor.Lifetime == ServiceLifetime.Singleton)), Times.Once);
        }
    }
}