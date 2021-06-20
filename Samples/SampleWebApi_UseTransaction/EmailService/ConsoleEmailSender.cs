using System;
using System.Threading;
using System.Threading.Tasks;

namespace SampleWebApi_UseTransaction.EmailService
{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendAsync(Email email, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"++ ++ ++ ++ ++{Environment.NewLine}" +
                              $"Sender : {email.Sender}{Environment.NewLine}" +
                              $"Receiver : {email.Receiver}{Environment.NewLine}" +
                              $"Subject : {email.Subject}{Environment.NewLine}" +
                              $"Content : {email.Content}{Environment.NewLine}" +
                              $"++ ++ ++ ++ ++");

            return Task.CompletedTask;
        }
    }
}