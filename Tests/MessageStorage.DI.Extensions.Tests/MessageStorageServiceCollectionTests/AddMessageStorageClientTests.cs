using System;
using System.Collections.Generic;
using MessageStorage.Clients;
using MessageStorage.DI.Extension;
using MessageStorage.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MessageStorage.DI.Extensions.Tests.MessageStorageServiceCollectionTests
{
    public class AddMessageStorageClientTests
    {
        private MessageStorageServiceCollection _sut;
        private ServiceCollection _serviceCollection;

        [SetUp]
        public void SetUp()
        {
            _serviceCollection = new ServiceCollection();
            _sut = new MessageStorageServiceCollection(_serviceCollection);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void WhenAddMessageStorageClient_WithoutType__DerivedTypeShouldBeInjected()
        {
            _sut.AddMessageStorageClient(provider => new DummyMessageStorageClient());

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var dummyMessageStorageClient = serviceProvider.GetService<DummyMessageStorageClient>();
                Assert.NotNull(dummyMessageStorageClient);

                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.Null(messageStorageClient);
            }
        }

        [Test]
        public void WhenAddMessageStorageClient_WithType__DefinedTypeShouldBeInjected()
        {
            _sut.AddMessageStorageClient<IMessageStorageClient>(provider => new DummyMessageStorageClient());

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var dummyMessageStorageClient = serviceProvider.GetService<DummyMessageStorageClient>();
                Assert.Null(dummyMessageStorageClient);

                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.NotNull(messageStorageClient);
            }
        }

        public class DummyMessageStorageClient : IMessageStorageClient
        {
            public Tuple<Message, IEnumerable<Job>> Add<T>(T payload)
            {
                throw new NotImplementedException();
            }

            public Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation)
            {
                throw new NotImplementedException();
            }
        }
    }
}