using BusinessLogic.Interfaces;
using BusinessLogic.Options;
using FluentResults;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessLogic.Services;

// Should be replaced with some valid email sender api like SendGrid

public class MailService(IOptions<MailOptions> options, ILogger<MailService> logger) : IMailService
{
    private readonly MailOptions _options = options.Value;

    public async Task<Result> SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            BodyBuilder builder = new() { HtmlBody = body };
            var emailMessage = new MimeMessage
            {
                Subject = subject,
                Body = builder.ToMessageBody()
            };
            emailMessage.From.Add(new MailboxAddress(_options.UserName, _options.EmailAddress));
            emailMessage.To.Add(new MailboxAddress(string.Empty, email));

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.SmtpServer, _options.SmtpPort, useSsl: true);
            await client.AuthenticateAsync(_options.EmailAddress, _options.Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
            logger.LogTrace("Sending email from {from} to {to} with subject {subject}", _options.EmailAddress, email, subject);
            logger.LogDebug("Body: {body}", body);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError("{Message}", ex.Message);
            return Result.Fail("Unable to send email");
        }
    }
}
