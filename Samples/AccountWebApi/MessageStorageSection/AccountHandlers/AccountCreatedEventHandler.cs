using System;
using System.Threading;
using System.Threading.Tasks;
using AccountWebApi.MessageStorageSection.Messages;
using MessageStorage;

namespace AccountWebApi.MessageStorageSection.AccountHandlers
{
    public class AccountCreatedEventHandler : Handler<AccountCreatedEvent>
    {
        protected override Task HandleAsync(AccountCreatedEvent payload, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{payload} is handled by {nameof(AccountCreatedEventHandler)}");
            return Task.CompletedTask;
        }
    }
}