using System;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MessageStorage.DI.ExtensionsUnitTests
{
    public class MessageStorageServiceCollection_AddTests
    {
        private readonly MessageStorageServiceCollection _sut;
        private readonly Mock<IServiceCollection> _mockServiceCollection;

        public MessageStorageServiceCollection_AddTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _sut = new MessageStorageServiceCollection(_mockServiceCollection.Object);
        }

        [Fact]
        public void WhenAddMethodExecuted_Singleton__IServiceCollectionAddMethodShouldBeExecuted()
        {
            _sut.Add(provider => provider.GetService<string>(), ServiceLifetime.Singleton);

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton)), Times.Once);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationFactory.GetType() == typeof(Func<IServiceProvider, string>))), Times.Once);
        }

        [Fact]
        public void WhenAddMethodExecuted_Scoped__IServiceCollectionAddMethodShouldBeExecuted()
        {
            _sut.Add(provider => provider.GetService<string>(), ServiceLifetime.Scoped);

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Scoped)), Times.Once);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationFactory.GetType() == typeof(Func<IServiceProvider, string>))), Times.Once);
        }

        [Fact]
        public void WhenAddMethodExecuted_Transient__IServiceCollectionAddMethodShouldBeExecuted()
        {
            _sut.Add(provider => provider.GetService<string>(), ServiceLifetime.Transient);

            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.Lifetime == ServiceLifetime.Transient)), Times.Once);
            _mockServiceCollection.Verify(collection => collection.Add(It.Is<ServiceDescriptor>(descriptor => descriptor.ImplementationFactory.GetType() == typeof(Func<IServiceProvider, string>))), Times.Once);
        }

        [Fact]
        public void WhenAddMethodExecuted_NotValidLifetime__IArgumentOutOfRangeExceptionOccurs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Add(provider => provider.GetService<string>(), (ServiceLifetime) 1000));
        }
    }
}