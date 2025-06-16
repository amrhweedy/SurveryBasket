using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
 using Microsoft.Extensions.Options;
using MimeKit;
using SurveyBasket.Api.Settings;

namespace SurveyBasket.Api.Services.Emails;

public class EmailService(IOptions<MailSettings> options ) : IEmailSender
{
    private readonly MailSettings _options = options.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_options.Mail),
            Subject = subject
            
        };

        message.To.Add(MailboxAddress.Parse(email));

        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        client.Connect(_options.Host, _options.Port, SecureSocketOptions.StartTls);
        client.Authenticate(_options.Mail,_options.Password);
        await  client.SendAsync(message);
        client.Disconnect(true);

    }

 }

