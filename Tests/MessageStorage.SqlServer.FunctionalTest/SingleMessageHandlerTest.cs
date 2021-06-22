using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.SqlServer.FunctionalTest.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class SingleMessageHandlerTest
    {
        private readonly TestServerFixture _testServerFixture;

        public SingleMessageHandlerTest(TestServerFixture testServerFixture)
        {
            _testServerFixture = testServerFixture;
        }

        [Theory]
        [InlineData(true, JobStatus.Failed)]
        [InlineData(false, JobStatus.Completed)]
        public async Task When_MessageHandlerExecuted__JobStatusChanged(bool throwException, JobStatus expectedResult)
        {
            using IServiceScope serviceScope = _testServerFixture.GetServiceScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var outboxClient = serviceProvider.GetRequiredService<IMessageStorageClient>();
            OrderSubmitted orderSubmitted = new OrderSubmitted(Guid.NewGuid(), throwException);

            var (_, jobs) = await outboxClient.AddMessageAsync(orderSubmitted);
            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await AsyncHelper.WaitFor(_testServerFixture.WaitAfterJobNotHandled, 2);

            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            RepositoryConfiguration? repositoryConfiguration = repositoryFactory.RepositoryConfiguration;

            await using var connection = new SqlConnection(repositoryConfiguration.ConnectionString);
            string script = $"select JobStatus from {repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? result = await connection.QueryFirstAsync(script, new {id = job.Id});

            Assert.Equal((int) expectedResult, result.JobStatus);
        }

        public class OrderSubmitted
        {
            public Guid OrderId { get; private set; }
            public bool ThrowException { get; private set; }

            public OrderSubmitted(Guid orderId, bool throwException)
            {
                OrderId = orderId;
                ThrowException = throwException;
            }
        }

        public class OrderSubmittedMessageHandler : BaseMessageHandler<OrderSubmitted>
        {
            public override Task HandleAsync(OrderSubmitted payload, CancellationToken cancellationToken = default)
            {
                if (payload.ThrowException)
                {
                    throw new ApplicationException($"{payload.OrderId} -- throws exception :)");
                }

                return Task.CompletedTask;
            }
        }
    }
}