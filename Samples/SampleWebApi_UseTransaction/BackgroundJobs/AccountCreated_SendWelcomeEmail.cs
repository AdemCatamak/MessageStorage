using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;
using SampleWebApi_UseTransaction.EmailService;

namespace SampleWebApi_UseTransaction.BackgroundJobs
{
    public class AccountCreated_SendWelcomeEmail
        : BaseMessageHandler<AccountCreated>
    {
        private readonly IEmailSender _emailSender;

        public AccountCreated_SendWelcomeEmail(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public override async Task HandleAsync(AccountCreated payload, CancellationToken cancellationToken = default)
        {
            Email email = new Email(payload.Email, "Welcome", $"Your account created on : {payload.CreatedOn} with Id : {payload.AccountId}");
            await _emailSender.SendAsync(email, cancellationToken);
        }
    }
}