namespace AccountWebApi.MessageStorageSection.Messages
{
    public class AccountCreatedEvent : AccountEvent
    {
        public long AccountId { get; private set; }

        public AccountCreatedEvent(long accountId, string email) : base(email)
        {
            AccountId = accountId;
        }
    }
}