namespace AccountWebApi.AccountApiMessageStorageSection.Messages
{
    public abstract class AccountEvent
    {
        public string Email { get; set; }

        public override string ToString()
        {
            return $"Email: {Email} || {GetType().Name}";
        }
    }
}