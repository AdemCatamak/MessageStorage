using System.Collections.Generic;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.DataAccessLayer;
using MessageStorage.Db.DataAccessLayer.Repositories;
using MessageStorage.Exceptions;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.DbRepositoryResolverTests
{
    public class DbRepository_Resolve_Test
    {
        private DbRepositoryResolver _sut;

        private List<IDbRepository> _dbRepositories;

        [SetUp]
        public void SetUp()
        {
            _dbRepositories = new List<IDbRepository>();
            _sut = new DbRepositoryResolver(_dbRepositories);
        }

        [Test]
        public void WhenListIsEmpty__RepositoryNotFoundExceptionOccurs()
        {
            Assert.Throws<RepositoryNotFoundException>(() => _sut.Resolve<IMessageRepository>());
        }

        [Test]
        public void WhenListDoesNotContainsDemandedRepository__RepositoryNotFoundExceptionOccurs()
        {
            _dbRepositories.AddRange(new List<IDbRepository>
                                     {
                                         new Mock<IDbMessageRepository>().Object
                                     });

            Assert.Throws<RepositoryNotFoundException>(() => _sut.Resolve<IDbJobRepository>());
            Assert.Throws<RepositoryNotFoundException>(() => _sut.Resolve<IJobRepository>());
        }

        [Test]
        public void WhenListContainsConcreteRepository__ResponseShouldNotBeNull()
        {
            _dbRepositories.AddRange(new List<IDbRepository>
                                     {
                                         new Mock<IDbMessageRepository>().Object
                                     });

            var dbMessageRepository = _sut.Resolve<IDbMessageRepository>();

            Assert.NotNull(dbMessageRepository);
        }

        [Test]
        public void WhenListContainsConcreteRepository_And_BaseTypeIsDemanded__ResponseShouldNotBeNull()
        {
            _dbRepositories.AddRange(new List<IDbRepository>
                                     {
                                         new Mock<IDbMessageRepository>().Object
                                     });

            var anyRepository = _sut.Resolve<IRepository>();

            Assert.NotNull(anyRepository);
        }
    }
}