using System;
using System.Threading.Tasks;
using MessageStorage;

namespace AccountWebApi.AccountApiMessageStorageSection.AccountHandlers
{
    public class AccountCreatedEvent : AccountEvent
    {
        public long SampleModelId { get; set; }
        
        public override string ToString()
        {
            return $"EventType: {this.GetType().Name} | Email: {Email}";
        }
    }

    public class AccountCreatedEventHandler : Handler<AccountCreatedEvent>
    {
        protected override Task Handle(AccountCreatedEvent payload)
        {
            Console.WriteLine($"{payload} handled by {nameof(AccountCreatedEventHandler)}");
            return Task.CompletedTask;
        }
    }
}