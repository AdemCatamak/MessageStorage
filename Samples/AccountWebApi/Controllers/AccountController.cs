using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using AccountWebApi.AccountApiMessageStorageSection.AccountHandlers;
using AccountWebApi.EntityFrameworkSection;
using AccountWebApi.EntityFrameworkSection.Models;
using MessageStorage.Db.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountWebApi.Controllers
{
    [Route("accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;
        private readonly AccountDbContext _accountDbContext;


        public AccountController(IMessageStorageDbClient messageStorageDbClient, AccountDbContext accountDbContext)
        {
            _messageStorageDbClient = messageStorageDbClient;
            _accountDbContext = accountDbContext;
        }

        [HttpPost("")]
        public IActionResult PostAccount([FromBody] string email)
        {
            var accountModel = new AccountModel(email);
            using (DbTransaction transaction = _accountDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
            {
                _accountDbContext.Accounts.Add(accountModel);
                _accountDbContext.SaveChanges();

                _messageStorageDbClient.UseTransaction(transaction);
                var accountCreatedEvent = new AccountCreatedEvent
                                          {
                                              SampleModelId = accountModel.Id,
                                              Email = email
                                          };

                _messageStorageDbClient.Add(accountCreatedEvent);

                transaction.Commit();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}