using System.Threading.Tasks;

namespace AddOptimization.Utilities.Interface;

public interface IEmailService
{
    Task<bool> SendEmail(string recipientEmails, string subject, string body, string cc = null, bool hasHtml = true);
}
