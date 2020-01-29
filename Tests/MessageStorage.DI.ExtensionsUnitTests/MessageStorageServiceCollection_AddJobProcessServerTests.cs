using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests
{
    public class MessageStorageServiceCollection_AddJobProcessServerTests
    {
        private readonly MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public MessageStorageServiceCollection_AddJobProcessServerTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddHandlerManagerMethodExecuted__IHandlerManager_and_IHandlerManagerConcrete_ShouldBeInjected()
        {
            _sut.AddJobProcessServer();

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton)), Times.AtLeast(2));
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ServiceType == typeof(IJobProcessServer))), Times.AtLeastOnce);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationType == typeof(JobProcessServer))), Times.AtLeastOnce);
        }
    }
}