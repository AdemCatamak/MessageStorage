using System;
using System.Threading.Tasks;
using AccountWebApi.AccountApiMessageStorageSection.Messages;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public class AccountEventHandler : Handler<AccountEvent>
    {
        protected override Task Handle(AccountEvent payload)
        {
            Console.WriteLine($"{payload} handled by {nameof(AccountEventHandler)}");
            return Task.CompletedTask;
        }
    }
}