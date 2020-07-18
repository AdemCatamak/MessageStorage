using System;
using System.Threading.Tasks;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public class AccountCreatedEvent : AccountEvent
    {
        public long SampleModelId { get; set; }
    }

    public class AccountCreatedEventHandler : Handler<AccountCreatedEvent>
    {
        protected override Task Handle(AccountCreatedEvent payload)
        {
            Console.WriteLine($"{nameof(AccountCreatedEventHandler)} || Account: {payload.Email} | Event: {payload.GetType().Name}");
            return Task.CompletedTask;
        }
    }
}