using System;
using System.Threading.Tasks;
using MessageStorage;
using Samples.Db.WebApi.Events;

namespace Samples.Db.WebApi.Handlers
{
    public class CreatedEventHandler : Handler<ICreatedEvent>
    {
        protected override Task Handle(ICreatedEvent payload)
        {
            Console.WriteLine($"{nameof(CreatedEventHandler)}{Environment.NewLine}" +
                              payload.ToString());
            return Task.CompletedTask;
        }
    }
}