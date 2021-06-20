using MessageStorage.DataAccessLayer;
using MessageStorage.DependencyInjection;
using MessageStorage.SqlServer;
using MessageStorage.SqlServer.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Forgetty.SqlServer.DependencyInjection.UnitTest
{
    public class InjectionExtension_UseSqlServer_Test
    {
        [Fact]
        public void When_UseSqlServer_IsUsed__ServiceCollectionContainsSqlServerRepositoryFactory()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddForgetty(configurator => configurator.UseSqlServer("connectionStr"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            var sqlServerRepositoryFactory = serviceProvider.GetService<ISqlServerRepositoryFactory>();

            Assert.NotNull(repositoryFactory);
            Assert.NotNull(sqlServerRepositoryFactory);
        }

        [Fact]
        public void When_UseSqlServer_IsUsed_with_Schema_ServiceCollectionContainsSqlServerRepositoryFactory()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddForgetty(configurator => configurator.UseSqlServer("connectionStr", "schema"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            var sqlServerRepositoryFactory = serviceProvider.GetService<ISqlServerRepositoryFactory>();

            Assert.NotNull(repositoryFactory);
            Assert.NotNull(sqlServerRepositoryFactory);
        }
    }
}