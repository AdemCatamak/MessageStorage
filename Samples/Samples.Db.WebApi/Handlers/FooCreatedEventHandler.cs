using System;
using System.Threading.Tasks;
using MessageStorage;
using Samples.Db.WebApi.Events;

namespace Samples.Db.WebApi.Handlers
{
    public class FooCreatedEventHandler : Handler<FooCreatedEvent>
    {
        protected override Task Handle(FooCreatedEvent payload)
        {
            Console.WriteLine(payload.ToString());
            return Task.CompletedTask;
        }
    }
}