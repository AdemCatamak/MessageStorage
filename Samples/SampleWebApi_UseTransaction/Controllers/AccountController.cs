﻿using System.Data;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using SampleWebApi_UseTransaction.BackgroundJobs;
using SampleWebApi_UseTransaction.DataAccess;

namespace SampleWebApi_UseTransaction.Controllers;

[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMessageStorageClient _messageStorageClient;

    public AccountController(IConnectionFactory connectionFactory, IMessageStorageClient messageStorageClient)
    {
        _connectionFactory = connectionFactory;
        _messageStorageClient = messageStorageClient;
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] PostAccountHttpRequest? accountHttpRequest)
    {
        if (string.IsNullOrEmpty(accountHttpRequest?.Email)) return BadRequest("Email should be given");

        var account = new Account(accountHttpRequest.Email);

        using (IDbConnection connection = _connectionFactory.CreateConnection())
        {
            using IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            await connection.ExecuteAsync("insert into use_transaction_schema.accounts (account_id, email, created_on) values (@account_id, @email, @created_on)",
                                          new
                                          {
                                              account_id = account.AccountId,
                                              email = account.Email,
                                              created_on = account.CreatedOn
                                          },
                                          dbTransaction);


            var accountCreated = new AccountCreated(account.AccountId, account.Email, account.CreatedOn);
            using IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.UseTransaction(dbTransaction);
            await _messageStorageClient.AddMessageAsync(accountCreated);

            await messageStorageTransaction.CommitAsync(CancellationToken.None);
        }

        return StatusCode((int) HttpStatusCode.Created, account);
    }
}