using System;
using System.Threading.Tasks;
using MessageStorage;

namespace SampleWebApi.WebApiMessageStorageSection.SampleHandlers
{
    public class SampleMessage
    {
        public string Text { get; set; }
    }

    public class SampleMessageHandler : Handler<SampleMessage>
    {
        protected override Task Handle(SampleMessage payload)
        {
            Console.WriteLine(payload.Text);
            return Task.CompletedTask;
        }
    }
}