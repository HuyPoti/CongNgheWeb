using System.Net;
using System.Net.Mail;

namespace backend.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = config["Email:SmtpHost"]!;
        var smtpPort = int.Parse(config["Email:SmtpPort"]!);
        var senderEmail = config["Email:SenderEmail"]!;
        var senderPassword = config["Email:SenderPassword"]!;
        var senderName = config["Email:SenderName"] ?? "ZX Team";

        var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}