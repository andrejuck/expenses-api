namespace Transactions.Api.DataContracts;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}