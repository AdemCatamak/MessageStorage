using MessageStorage.DataAccessLayer;
using MessageStorage.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MessageStorage.Postgres.DependencyInjection.UnitTest
{
    public class InjectionExtension_UsePostgres_Test
    {
        [Fact]
        public void When_UsePostgres_IsUsed__ServiceCollectionContainsPostgresRepositoryFactory()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddMessageStorage(configurator => configurator.UsePostgres("connectionStr"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            var sqlServerRepositoryFactory = serviceProvider.GetService<IPostgresRepositoryFactory>();

            Assert.NotNull(repositoryFactory);
            Assert.NotNull(sqlServerRepositoryFactory);
        }

        [Fact]
        public void When_UsePostgres_IsUsed_with_Schema_ServiceCollectionContainsPostgresRepositoryFactory()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddMessageStorage(configurator => configurator.UsePostgres("connection-str", "schema"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            var sqlServerRepositoryFactory = serviceProvider.GetService<IPostgresRepositoryFactory>();

            Assert.NotNull(repositoryFactory);
            Assert.NotNull(sqlServerRepositoryFactory);
        }
    }
}