using System;
using System.Threading.Tasks;
using AccountWebApi.AccountApiMessageStorageSection.Messages;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public class AccountCreatedEventHandler : Handler<AccountCreatedEvent>
    {
        protected override Task Handle(AccountCreatedEvent payload)
        {
            Console.WriteLine($"{payload} handled by {nameof(AccountCreatedEventHandler)}");
            return Task.CompletedTask;
        }
    }
}