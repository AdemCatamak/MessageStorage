using System;
using System.Collections.Generic;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MessageStorage.DI.Extensions.Tests.MessageStorageServiceCollectionTests
{
    public class MessageStorageServiceCollection_AddRepositoryContext
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
        public void WhenAddRepositoryContext_WithoutType__DerivedTypeShouldBeInjected()
        {
            _sut.AddRepositoryContext<DummyRepositoryContext, DummyRepositoryConfiguration>(provider => new DummyRepositoryContext());

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var dummyRepositoryContext = serviceProvider.GetService<DummyRepositoryContext>();
                Assert.NotNull(dummyRepositoryContext);

                var repositoryContext1 = serviceProvider.GetService<IRepositoryContext<RepositoryConfiguration>>();
                Assert.Null(repositoryContext1);
                
                var repositoryContext2 = serviceProvider.GetService<IRepositoryContext<DummyRepositoryConfiguration>>();
                Assert.Null(repositoryContext2);
            }
        }

        [Test]
        public void WhenAddMessageStorageClient_WithBaseType__DefinedTypeShouldBeInjected()
        {
            _sut.AddRepositoryContext<IRepositoryContext<RepositoryConfiguration>, RepositoryConfiguration>(provider => new DummyRepositoryContext());

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var dummyRepositoryContext = serviceProvider.GetService<DummyRepositoryContext>();
                Assert.Null(dummyRepositoryContext);

                var repositoryContext1 = serviceProvider.GetService<IRepositoryContext<RepositoryConfiguration>>();
                Assert.NotNull(repositoryContext1);
                
                var repositoryContext2 = serviceProvider.GetService<IRepositoryContext<DummyRepositoryConfiguration>>();
                Assert.Null(repositoryContext2);
            }
        }
        
        [Test]
        public void WhenAddMessageStorageClient_WithType__DefinedTypeShouldBeInjected()
        {
            _sut.AddRepositoryContext<IRepositoryContext<DummyRepositoryConfiguration>, RepositoryConfiguration>(provider => new DummyRepositoryContext());

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var dummyRepositoryContext = serviceProvider.GetService<DummyRepositoryContext>();
                Assert.Null(dummyRepositoryContext);

                var repositoryContext1 = serviceProvider.GetService<IRepositoryContext<RepositoryConfiguration>>();
                Assert.Null(repositoryContext1);
                
                var repositoryContext2 = serviceProvider.GetService<IRepositoryContext<DummyRepositoryConfiguration>>();
                Assert.NotNull(repositoryContext2);
            }
        }

        public class DummyRepositoryConfiguration : RepositoryConfiguration
        {
        }

        public class DummyRepositoryContext : IRepositoryContext<DummyRepositoryConfiguration>
        {
            public Tuple<Message, IEnumerable<Job>> Add<T>(T payload)
            {
                throw new NotImplementedException();
            }

            public Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation)
            {
                throw new NotImplementedException();
            }

            public DummyRepositoryConfiguration RepositoryConfiguration { get; }
            public IMessageRepository<DummyRepositoryConfiguration> MessageRepository { get; }
            public IJobRepository<DummyRepositoryConfiguration> JobRepository { get; }
        }
    }
}