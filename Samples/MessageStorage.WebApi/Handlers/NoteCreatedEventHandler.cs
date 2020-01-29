using System;
using System.Threading.Tasks;

namespace MessageStorage.WebApi.Handlers
{
    public interface IEntityCreatedEvent
    {
        public string Id { get; set; }
    }

    public class NoteCreatedEvent : IEntityCreatedEvent
    {
        public string Id { get; set; }
        public string Note { get; set; }
    }

    public class EntityCreatedEventHandler : Handler<IEntityCreatedEvent>
    {
        protected override Task Handle(IEntityCreatedEvent payload)
        {
            Console.WriteLine($"IEntityCreatedEvent handled by {nameof(EntityCreatedEventHandler)}{Environment.NewLine}" +
                              $"{payload.Id}");

            return Task.CompletedTask;
        }
    }

    public class NoteCreatedEventHandler : Handler<NoteCreatedEvent>
    {
        protected override Task Handle(NoteCreatedEvent payload)
        {
            Console.WriteLine($"Note is handled by {nameof(NoteCreatedEventHandler)}{Environment.NewLine}" +
                              $"{payload.Note}");

            return Task.CompletedTask;
        }
    }
}