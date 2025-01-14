using System.Net;
using System.Net.Mail;
using Expenses.Api.DataContracts;
using Expenses.Api.Settings;
using Microsoft.Extensions.Options;

namespace Expenses.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailingSettings _configuration;

    public EmailService(IOptions<EmailingSettings> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpClient = new SmtpClient(_configuration.Host, _configuration.Port)
        {
            Credentials = new NetworkCredential(_configuration.Username, _configuration.Password),
            EnableSsl = _configuration.EnableSSL
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}