using System.Data;
using MessageStorage.Clients;
using MessageStorage.DataAccessSection;
using MessageStorage.DI.Extension;
using MessageStorage.SqlServer.DI.Extension;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTest.MessageStorage.SqlServer.DI.Extension
{
    public class AddMessageStorageTest : IClassFixture<SqlServerTestFixture>
    {
        [Fact]
        public void WhenAddMessageStorage_WithJobProcessor__IMessageStorageClientAndIBackgroundJobProcessorShouldBeResolved()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMessageStorage(builder =>
                                                    builder.UseSqlServer(SqlServerTestFixture.CONNECTION_STR)
                                               )
                             .WithJobProcessor();

            using (ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                var backgroundProcessor = serviceProvider.GetService<IBackgroundProcessor>();

                Assert.NotNull(messageStorageClient);
                Assert.NotNull(backgroundProcessor);
            }
        }

        [Fact]
        public void WhenAddMessageStorage_WithoutJobProcessor__IBackgroundJobProcessorShouldNotBeResolved()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMessageStorage(builder =>
                                                    builder.UseSqlServer(SqlServerTestFixture.CONNECTION_STR)
                                               );

            using (ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                var backgroundProcessor = serviceProvider.GetService<IBackgroundProcessor>();

                Assert.NotNull(messageStorageClient);
                Assert.Null(backgroundProcessor);
            }
        }

        [Fact]
        public void WhenUseSqlServer__DbTransactionShouldBeSqlTransaction()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMessageStorage(builder =>
                                                    builder.UseSqlServer(SqlServerTestFixture.CONNECTION_STR)
                                               );

            using (ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetRequiredService<IMessageStorageClient>();

                using (IMessageStorageTransaction transaction = messageStorageClient.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    IDbTransaction dbTransaction = transaction.ToDbTransaction();
                    Assert.Equal(typeof(SqlTransaction), dbTransaction.GetType());
                }
            }
        }
    }
}