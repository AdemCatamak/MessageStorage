using System;
using System.Threading;
using System.Threading.Tasks;
using AccountWebApi.MessageStorageSection.Messages;
using MessageStorage;

namespace AccountWebApi.MessageStorageSection.AccountHandlers
{
    public class AccountEventHandler : Handler<AccountEvent>
    {
        protected override Task HandleAsync(AccountEvent payload, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{payload} is handled by {nameof(AccountEventHandler)}");
            return Task.CompletedTask;
        }
    }
}