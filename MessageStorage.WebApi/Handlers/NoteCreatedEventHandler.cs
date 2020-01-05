using System;
using System.Threading.Tasks;

namespace MessageStorage.WebApi.Handlers
{
    public class NoteCreatedEvent
    {
        public string Note { get; set; }
    }

    public class NoteCreatedEventHandler : Handler<NoteCreatedEvent>
    {
        public override Task Handle(NoteCreatedEvent payload)
        {
            Console.WriteLine($"Note is handled by {nameof(NoteCreatedEventHandler)}{Environment.NewLine}" +
                              $"{payload.Note}");

            return Task.CompletedTask;
        }
    }
}