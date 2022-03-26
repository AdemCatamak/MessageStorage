using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.Extensions;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using TestUtility.DbUtils;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.MessageStorageClientTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class AddMessageWithTransactionTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;

    public AddMessageWithTransactionTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
    }

    [Fact(Timeout = 1000)]
    public async Task WhenBorrowedTransactionNotCommitted_MessageAndJobShouldNotBeInserted()
    {
        Message message;
        Job job;
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            await connection.OpenAsync();
            using (DbTransaction transaction = await connection.BeginTransactionAsync())
            using (IMessageStorageTransaction _ = _messageStorageClient.UseTransaction(transaction))
            {
                var basicMessage = new BasicMessage("some-message");
                (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
                Assert.Single(jobs);
                job = jobs.First();
                Assert.Equal(JobStatus.InProgress, job.JobStatus);
            }
        }

        Message? messageFromDb = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.Null(messageFromDb);

        Job? jobFromDb = await Db.Fetch.JobFromSqlServerAsync(job.Id);
        Assert.Null(jobFromDb);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenTransactionNotCommitted_MessageAndJobShouldNotBeInserted()
    {
        Message message;
        Job job;
        using (IMessageStorageTransaction _ = _messageStorageClient.StartTransaction())
        {
            var basicMessage = new BasicMessage("some-message");
            (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
            Assert.Single(jobs);
            job = jobs.First();
            Assert.Equal(JobStatus.InProgress, job.JobStatus);
        }

        Message? messageFromDb = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.Null(messageFromDb);

        Job? jobFromDb = await Db. Fetch.JobFromSqlServerAsync(job.Id);
        Assert.Null(jobFromDb);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenBorrowedTransactionCommitted_MessageAndJobShouldBeInserted()
    {
        Message message;
        Job job;
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            await connection.OpenAsync();
            using (DbTransaction transaction = await connection.BeginTransactionAsync())
            using (IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.UseTransaction(transaction))
            {
                var basicMessage = new BasicMessage("some-message");
                (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
                Assert.Single(jobs);
                job = jobs.First();
                Assert.Equal(JobStatus.InProgress, job.JobStatus);
                await messageStorageTransaction.CommitAsync();
            }
        }

        Message? messageFromDb = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDb);

        Job? jobFromDb = await Db.Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDb);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenTransactionCommitted_MessageAndJobShouldBeInserted()
    {
        Message message;
        Job job;
        using (IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.StartTransaction())
        {
            var basicMessage = new BasicMessage("some-message");
            (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
            Assert.Single(jobs);
            job = jobs.First();
            Assert.Equal(JobStatus.InProgress, job.JobStatus);
            await messageStorageTransaction.CommitAsync(CancellationToken.None);
        }

        Message? messageFromDb = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDb);

        Job? jobFromDb = await Db.Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDb);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenSeperatedTransactionIsOpened_ProcessResultShouldNotEffectEachOther()
    {
        Message message1;
        Job job1;
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            await connection.OpenAsync();
            using (DbTransaction transaction = await connection.BeginTransactionAsync())
            using (IMessageStorageTransaction _ = _messageStorageClient.UseTransaction(transaction))
            {
                var basicMessage = new BasicMessage("some-message");
                (message1, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
                Assert.Single(jobs);
                job1 = jobs.First();
                Assert.Equal(JobStatus.InProgress, job1.JobStatus);
            }
        }

        Message message2;
        Job job2;
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            await connection.OpenAsync();
            using (DbTransaction transaction = await connection.BeginTransactionAsync())
            using (IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.UseTransaction(transaction))
            {
                var basicMessage = new BasicMessage("some-message");
                (message2, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);
                Assert.Single(jobs);
                job2 = jobs.First();
                Assert.Equal(JobStatus.InProgress, job2.JobStatus);
                await messageStorageTransaction.CommitAsync();
            }
        }

        Message? message1FromDb = await Db.Fetch.MessageFromSqlServerAsync(message1.Id);
        Assert.Null(message1FromDb);

        Job? job1FromDb = await Db.Fetch.JobFromSqlServerAsync(job1.Id);
        Assert.Null(job1FromDb);

        Message? message2FromDb = await Db.Fetch.MessageFromSqlServerAsync(message2.Id);
        Assert.NotNull(message2FromDb);

        Job? job2FromDb = await Db.Fetch.JobFromSqlServerAsync(job2.Id);
        Assert.NotNull(job2FromDb);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenStartTransactionIsUsed_MultipleMessageShouldBeInsertedUnderSameTransaction()
    {
        Message message1;
        Message message2;
        using (IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.StartTransaction())
        {
            var basicMessage1 = new BasicMessage("some-message");
            var basicMessage2 = new BasicMessage("some-message");
            (message1, List<Job> jobList1) = await _messageStorageClient.AddMessageAsync(basicMessage1);
            (message2, List<Job> jobList2) = await _messageStorageClient.AddMessageAsync(basicMessage2);
            Assert.Single(jobList1);
            Assert.Single(jobList2);
            Job job1 = jobList1.First();
            Job job2 = jobList2.First();
            Assert.Equal(JobStatus.InProgress, job1.JobStatus);
            Assert.Equal(JobStatus.InProgress, job2.JobStatus);

            Message? message1FromDbInner = await Db.Fetch.MessageFromSqlServerAsync(message1.Id);
            Assert.Null(message1FromDbInner);

            Message? message2FromDbInner = await Db.Fetch.MessageFromSqlServerAsync(message2.Id);
            Assert.Null(message2FromDbInner);

            await messageStorageTransaction.CommitAsync();
        }


        Message? message1FromDb = await Db.Fetch.MessageFromSqlServerAsync(message1.Id);
        Assert.NotNull(message1FromDb);

        Message? message2FromDb = await Db.Fetch.MessageFromSqlServerAsync(message2.Id);
        Assert.NotNull(message2FromDb);
    }


    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}