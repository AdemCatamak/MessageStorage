using System.Data;
using System.Data.Common;
using System.Net;
using AccountWebApi.EntityFrameworkSection;
using AccountWebApi.EntityFrameworkSection.Models;
using AccountWebApi.MessageStorageSection.Messages;
using AccountWebApi.SecondMessageStorageSection;
using MessageStorage.Clients;
using MessageStorage.DataAccessSection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountWebApi.Controllers
{
    [Route("accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly ISecondMessageStorageClient _secondMessageStorageClient;
        private readonly AccountDbContext _accountDbContext;


        public AccountController(IMessageStorageClient messageStorageClient, ISecondMessageStorageClient secondMessageStorageClient, AccountDbContext accountDbContext)
        {
            _messageStorageClient = messageStorageClient;
            _secondMessageStorageClient = secondMessageStorageClient;
            _accountDbContext = accountDbContext;
        }

        [HttpPost("")]
        public IActionResult PostAccount([FromBody] string email)
        {
            var accountModel = new AccountModel(email);
            using (DbTransaction transaction = _accountDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
            {
                IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.UseTransaction(transaction);

                _accountDbContext.Accounts.Add(accountModel);
                _accountDbContext.SaveChanges();

                var accountCreatedEvent = new AccountCreatedEvent(accountModel.Id, accountModel.Email);
                _messageStorageClient.Add(accountCreatedEvent);

                messageStorageTransaction.Commit();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }

        [HttpPost("second")]
        public IActionResult PostAccountSecond([FromBody] string email)
        {
            var accountModel = new AccountModel(email);
            using (DbTransaction transaction = _accountDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
            {
                IMessageStorageTransaction messageStorageTransaction = _secondMessageStorageClient.UseTransaction(transaction);

                _accountDbContext.Accounts.Add(accountModel);
                _accountDbContext.SaveChanges();

                var accountCreatedEvent = new AccountCreatedEvent(accountModel.Id, accountModel.Email);
                _secondMessageStorageClient.Add(accountCreatedEvent);

                messageStorageTransaction.Commit();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}