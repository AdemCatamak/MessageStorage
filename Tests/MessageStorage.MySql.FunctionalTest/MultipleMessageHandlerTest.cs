using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.FunctionalTest.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using TestUtility;
using Xunit;

namespace MessageStorage.MySql.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class MultipleMessageHandlerTest
    {
        private readonly TestServerFixture _testServerFixture;

        public MultipleMessageHandlerTest(TestServerFixture testServerFixture)
        {
            _testServerFixture = testServerFixture;
        }

        [Fact]
        public async Task When_MessageTypeCompatibleWithMultipleMessageHandler__JobsCreatedAndExecutedSeparately()
        {
            using IServiceScope serviceScope = _testServerFixture.GetServiceScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var outboxClient = serviceProvider.GetRequiredService<IMessageStorageClient>();

            var orderCreated = new OrderCreated(Guid.NewGuid(), "buyer-name");
            var (_, jobs) = await outboxClient.AddMessageAsync(orderCreated);
            var jobList = jobs.ToList();
            Assert.Equal(2, jobList.Count);

            await AsyncHelper.WaitFor(_testServerFixture.WaitAfterJobNotHandled, 2);

            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            RepositoryConfiguration repositoryConfiguration = repositoryFactory.RepositoryConfiguration;

            await using var connection = new MySqlConnection(repositoryConfiguration.ConnectionString);

            foreach (Job job in jobList)
            {
                string script = $"select job_status from {repositoryConfiguration.Schema}.jobs where id = @id";
                dynamic? result = await connection.QueryFirstAsync(script, new {id = job.Id});

                JobStatus expectedResult
                    = job.MessageHandlerTypeName == typeof(OrderCreated_SendEmail).FullName
                          ? JobStatus.Completed
                          : JobStatus.Failed;

                Assert.Equal((int) expectedResult, result.job_status);
            }
        }

        public class OrderCreated
        {
            public Guid Id { get; private set; }
            public string BuyerName { get; private set; }

            public OrderCreated(Guid id, string buyerName)
            {
                Id = id;
                BuyerName = buyerName;
            }
        }

        public class OrderCreated_SendEmail : BaseMessageHandler<OrderCreated>
        {
            public override Task HandleAsync(OrderCreated payload, CancellationToken cancellationToken = default)
            {
                if (Guid.Empty == payload.Id) throw new ValidationException("OrderId is empty");
                if (string.IsNullOrEmpty(payload.BuyerName)) throw new ValidationException("Buyer name is empty");

                return Task.CompletedTask;
            }
        }

        public class OrderCreated_PrepareBill : BaseMessageHandler<OrderCreated>
        {
            public override Task HandleAsync(OrderCreated payload, CancellationToken cancellationToken = default)
            {
                throw new ApplicationException("Exception is thrown");
            }
        }
    }
}