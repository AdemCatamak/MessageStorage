namespace Samples.Db.WebApi.Events
{
    public class SomethingCreatedEvent : ICreatedEvent
    {
        public string Id { get; set; }
        public string SomeField { get; set; }
    }
}