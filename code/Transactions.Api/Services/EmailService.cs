using Transactions.Api.DataContracts;
using Transactions.Api.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Transactions.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailingSettings _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailingSettings> configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
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
        _logger.LogInformation($"Confirmation email sent to {toEmail}");
    }
}