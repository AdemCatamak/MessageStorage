using System;
using System.Threading.Tasks;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public abstract class AccountEvent
    {
        public string Email { get; set; }

        public override string ToString()
        {
            return $"Email: {Email} || {GetType().Name}";
        }
    }

    public class AccountEventHandler : Handler<AccountEvent>
    {
        protected override Task Handle(AccountEvent payload)
        {
            Console.WriteLine($"{payload} handled by {nameof(AccountEventHandler)}");
            return Task.CompletedTask;
        }
    }
}