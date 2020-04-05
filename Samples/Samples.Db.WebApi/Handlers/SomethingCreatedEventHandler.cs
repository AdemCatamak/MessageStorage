using System;
using System.Threading.Tasks;
using MessageStorage;
using Samples.Db.WebApi.Events;

namespace Samples.Db.WebApi.Handlers
{
    public class SomethingCreatedEventHandler : Handler<SomethingCreatedEvent>
    {
        protected override Task Handle(SomethingCreatedEvent payload)
        {
            Console.WriteLine(payload.Id);
            return Task.CompletedTask;
        }
    }
}