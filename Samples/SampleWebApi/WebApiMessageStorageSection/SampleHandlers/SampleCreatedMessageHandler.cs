using System;
using System.Threading.Tasks;
using MessageStorage;

namespace SampleWebApi.WebApiMessageStorageSection.SampleHandlers
{
    public class SampleCreatedMessage : SampleMessage
    {
        public long SampleModelId { get; set; }
    }

    public class SampleCreatedMessageHandler : Handler<SampleCreatedMessage>
    {
        protected override Task Handle(SampleCreatedMessage payload)
        {
            Console.WriteLine($"{nameof(SampleCreatedMessageHandler)} handle message => #{payload.SampleModelId}# {payload.Text}");
            return Task.CompletedTask;
        }
    }
}