namespace AccountWebApi.MessageStorageSection.Messages
{
    public abstract class AccountEvent
    {
        public string Email { get; private set; }

        protected AccountEvent(string email)
        {
            Email = email;
        }

        public override string ToString()
        {
            return $"Email: {Email} || {GetType().Name}";
        }
    }
}