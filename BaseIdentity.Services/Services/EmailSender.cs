using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using BaseIdentity.Services.Interfaces;

namespace BaseIdentity.Services.Services;

public class EmailSender(IConfiguration config) : IEmailSender
{
    private readonly IConfiguration _config = config;

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        using var client = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]!))
        {
            Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Pass"]),
            EnableSsl = true
        };
        var mail = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]!),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mail.To.Add(to);
        await client.SendMailAsync(mail);
    }
}
