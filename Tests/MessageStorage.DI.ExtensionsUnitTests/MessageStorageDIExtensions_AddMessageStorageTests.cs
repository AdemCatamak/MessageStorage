using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests
{
    public class MessageStorageDIExtensions_AddMessageStorageTests
    {
        [Fact]
        public void WhenAddMessageStorageMethodExecuted__FunctionThatSuppliedFromParameterShouldExecuted()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();
            var executed = false;

            void ExecuteMethod()
            {
                executed = true;
                mockServiceCollection.Object.Add(new ServiceDescriptor(typeof(string), typeof(string), ServiceLifetime.Singleton));
            }

            mockServiceCollection.Object.AddMessageStorage(collection => ExecuteMethod());

            Assert.True(executed);
            mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor =>
                                                                                                   descriptor.ServiceType == typeof(string)
                                                                                                && descriptor.ImplementationType == typeof(string)
                                                                                                && descriptor.Lifetime == ServiceLifetime.Singleton)), Times.Once);
        }
    }
}