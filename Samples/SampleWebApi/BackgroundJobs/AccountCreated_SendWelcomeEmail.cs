using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;
using SampleWebApi.EmailService;

namespace SampleWebApi.BackgroundJobs;

public class AccountCreated_SendWelcomeEmail
    : BaseMessageHandler<AccountCreated>
{
    private readonly IEmailSender _emailSender;

    public AccountCreated_SendWelcomeEmail(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    protected override async Task HandleAsync(IMessageContext<AccountCreated> messageContext, CancellationToken cancellationToken)
    {
        var payload = messageContext.Message;
        Email email = new Email(payload.Email, "Welcome", $"Your account created on : {payload.CreatedOn} with Id : {payload.AccountId}");
        await _emailSender.SendAsync(email, cancellationToken);
    }
}