using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests.MessageStorageServiceCollectionTests
{
    public class MessageStorageServiceCollection_AddHandlerManagerTests
    {
        private readonly Extension.MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public MessageStorageServiceCollection_AddHandlerManagerTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new Extension.MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddHandlerManagerMethodExecuted__IHandlerManager_and_IHandlerManagerConcrete_ShouldBeInjected()
        {
            _sut.AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton);

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton)), Times.Exactly(2));
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationType == typeof(HandlerManager))), Times.Exactly(2));
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ServiceType == typeof(IHandlerManager))), Times.Once);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ServiceType == typeof(HandlerManager))), Times.Once);
        }
    }
}