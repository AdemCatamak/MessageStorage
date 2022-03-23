using System;
using System.Net;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.AspNetCore.Mvc;
using SampleWebApi.BackgroundJobs;

namespace SampleWebApi.Controllers;

[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IMessageStorageClient _messageStorageClient;

    public AccountController(IMessageStorageClient messageStorageClient)
    {
        _messageStorageClient = messageStorageClient;
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] PostAccountHttpRequest accountHttpRequest)
    {
        if (string.IsNullOrEmpty(accountHttpRequest?.Email)) return BadRequest("Email should be given");

        AccountCreated accountCreated = new AccountCreated(Guid.NewGuid(), accountHttpRequest.Email, DateTime.UtcNow);
        await _messageStorageClient.AddMessageAsync(accountCreated);

        return StatusCode((int) HttpStatusCode.Created, accountCreated);
    }
}