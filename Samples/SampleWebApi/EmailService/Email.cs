namespace SampleWebApi.EmailService
{
    public class Email
    {
        public string Receiver { get; }
        public string Sender { get; }
        public string Subject { get; }
        public string Content { get; }

        public Email(string receiver, string subject, string content, string? sender = null)
        {
            Receiver = receiver;
            Sender = sender ?? "messagestorage@messagestorage.com";
            Subject = subject;
            Content = content;
        }
    }
}