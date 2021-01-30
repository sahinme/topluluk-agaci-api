using System.Threading.Tasks;

namespace Microsoft.Nnn.ApplicationCore.Interfaces
{

    public interface IEmailSender
    {
        Task SendEmail(string email, string subject, string message);
    }
}
