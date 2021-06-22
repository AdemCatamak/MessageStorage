using System.Threading;
using System.Threading.Tasks;

namespace SampleWebApi.EmailService
{
    public interface IEmailSender
    {
        Task SendAsync(Email email, CancellationToken cancellationToken = default);
    }
}