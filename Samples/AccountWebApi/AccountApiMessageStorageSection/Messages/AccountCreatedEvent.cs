namespace AccountWebApi.AccountApiMessageStorageSection.Messages
{
    public class AccountCreatedEvent : AccountEvent
    {
        public long AccountId { get; set; }

        public override string ToString()
        {
            return $"Email: {Email} || {GetType().Name}";
        }
    }
}