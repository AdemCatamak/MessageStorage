using System;

namespace Samples.Db.WebApi.Events
{
    public class FooCreatedEvent : ICreatedEvent
    {
        public string Id { get; set; }
        public string SomeField { get; set; }

        public override string ToString()
        {
            string result = $"{nameof(FooCreatedEvent)}{Environment.NewLine}" +
                            $"{nameof(FooCreatedEvent)}.{nameof(Id)} : {Id}{Environment.NewLine}" +
                            $"{nameof(FooCreatedEvent)}.{nameof(SomeField)} : {SomeField}";

            return result;
        }
    }
}