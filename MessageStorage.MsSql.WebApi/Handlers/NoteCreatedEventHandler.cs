using System;
using System.Threading.Tasks;

namespace MessageStorage.MsSql.WebApi.Handlers
{
    public interface IEntityCreatedEvent
    {
        public string Id { get; set; }
    }

    public class NoteCreatedEvent : IEntityCreatedEvent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class EntityCreatedEventHandler : Handler<IEntityCreatedEvent>
    {
        protected override Task Handle(IEntityCreatedEvent payload)
        {
            Console.WriteLine($"IEntityCreatedEvent handled by {nameof(EntityCreatedEventHandler)}{Environment.NewLine}" +
                              $"{nameof(payload.Id)}:{payload.Id}");

            return Task.CompletedTask;
        }
    }
}