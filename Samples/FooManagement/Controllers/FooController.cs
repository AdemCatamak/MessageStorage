using FooManagement.Controllers.Requests;
using FooManagement.DataAccessLayer;
using FooManagement.MessageHandlers;
using FooManagement.SecondaryMessageStorageSection;
using MessageStorage;
using MessageStorage.DataAccessLayer;
using MessageStorage.Postgres.Extensions;
using MessageStorage.SqlServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace FooManagement.Controllers;

[ApiController]
[Route("foos")]
public class FooController : ControllerBase
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IPostgresConnectionFactory _postgresConnectionFactory;

    private readonly IMessageStorageClient _messageStorageClient;
    private readonly ISecondaryMessageStorageClient _secondaryMessageStorageClient;

    public FooController(ISqlConnectionFactory sqlConnectionFactory, IPostgresConnectionFactory postgresConnectionFactory, IMessageStorageClient messageStorageClient, ISecondaryMessageStorageClient secondaryMessageStorageClient)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _postgresConnectionFactory = postgresConnectionFactory;
        _messageStorageClient = messageStorageClient;
        _secondaryMessageStorageClient = secondaryMessageStorageClient;
    }

    [HttpPost("sql-server")]
    public async Task<IActionResult> CreateFooInSqlServerAsync([FromBody] PostFooRequest request, CancellationToken cancellationToken)
    {
        using (SqlConnection sqlConnection = await _sqlConnectionFactory.CreateAsync(cancellationToken))
        using (IMessageStorageTransaction messageStorageTransaction = sqlConnection.BeginTransaction(_messageStorageClient))
        {
            var fooCreatedMessage = new FooCreatedEvent(Guid.NewGuid().ToString(), request.StrValue);
            var fooCreatedIntegrationEvent = new FooCreatedIntegrationEvent(fooCreatedMessage.Id, fooCreatedMessage.StrValue);
            await _messageStorageClient.AddMessageAsync(fooCreatedMessage, cancellationToken);
            await _messageStorageClient.AddMessageAsync(fooCreatedIntegrationEvent, cancellationToken);

            await messageStorageTransaction.CommitAsync(cancellationToken);
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("postgres")]
    public async Task<IActionResult> CreateFooInPostgresAsync([FromBody] PostFooRequest request, CancellationToken cancellationToken)
    {
        using (NpgsqlConnection? npgsqlConnection = await _postgresConnectionFactory.CreateAsync(cancellationToken))
        using (IMessageStorageTransaction messageStorageTransaction = npgsqlConnection.BeginTransaction(_secondaryMessageStorageClient))
        {
            var fooCreatedMessage = new FooCreatedEvent(Guid.NewGuid().ToString(), request.StrValue);
            await _secondaryMessageStorageClient.AddMessageAsync(fooCreatedMessage, cancellationToken);
            await messageStorageTransaction.CommitAsync(cancellationToken);
        }

        return StatusCode(StatusCodes.Status201Created);
    }
}