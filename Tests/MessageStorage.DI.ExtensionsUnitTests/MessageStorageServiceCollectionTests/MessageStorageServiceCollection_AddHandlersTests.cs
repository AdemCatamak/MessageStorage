using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests.MessageStorageServiceCollectionTests
{
    public class MessageStorageServiceCollection_AddHandlersTests
    {
        private readonly Extension.MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public MessageStorageServiceCollection_AddHandlersTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new Extension.MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddHandlersMethodExecuted__Handler_and_HandlerConcrete_ShouldBeInjected()
        {
            _sut.AddHandlers(new[] {GetType().Assembly});

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton)), Times.AtLeast(2));
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ServiceType == typeof(Handler))), Times.AtLeastOnce);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationType == typeof(DummyHandler))), Times.AtLeastOnce);
        }

        public class DummyHandler : Handler<object>
        {
            protected override Task Handle(object payload)
            {
                throw new Exception("This exception should not be seen. Actually this class is not used");
            }
        }
    }
}