using System;
using System.Threading;
using System.Threading.Tasks;
using AccountWebApi.EntityFrameworkSection;
using AccountWebApi.EntityFrameworkSection.Models;
using AccountWebApi.MessageStorageSection.Messages;
using MessageStorage;
using Microsoft.EntityFrameworkCore;

namespace AccountWebApi.MessageStorageSection.AccountHandlers
{
    public class AccountCreatedEventHandler : Handler<AccountCreatedEvent>
    {
        private readonly AccountDbContext _accountDbContext;

        public AccountCreatedEventHandler(AccountDbContext accountDbContext)
        {
            _accountDbContext = accountDbContext;
        }

        protected override async Task HandleAsync(AccountCreatedEvent payload, CancellationToken cancellationToken)
        {
            long accountId = payload.AccountId;
            AccountModel accountModel = await _accountDbContext.Accounts.FirstAsync(x => x.Id == accountId, cancellationToken: cancellationToken);
            Console.WriteLine($"{payload} is handled by {nameof(AccountCreatedEventHandler)} || #{accountModel.Id} - {accountModel}");
        }
    }
}