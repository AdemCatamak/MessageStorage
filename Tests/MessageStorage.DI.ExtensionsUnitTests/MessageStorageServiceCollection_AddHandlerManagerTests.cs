using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests
{
    public class MessageStorageServiceCollection_AddHandlerManagerTests
    {
        private readonly MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public MessageStorageServiceCollection_AddHandlerManagerTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddHandlerManagerMethodExecuted__IHandlerManager_and_IHandlerManagerConcrete_ShouldBeInjected()
        {
            _sut.AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton);

            _mockServiceCollection.Verify(collection => collection.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }
    }
}