using System;
using System.Threading.Tasks;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public abstract class AccountEvent
    {
        public string Email { get; set; }
    }

    public class AccountEventHandler : Handler<AccountEvent>
    {
        protected override Task Handle(AccountEvent payload)
        {
            Console.WriteLine($"{nameof(AccountEventHandler)} || Account: {payload.Email} | Event: {payload.GetType().Name}");
            return Task.CompletedTask;
        }
    }
}