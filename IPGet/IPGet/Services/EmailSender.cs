using IPGet.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IPGet.Services
{
    public class EmailSender
    {
        //private readonly EmailSettings _emailSettings;
        private readonly SmtpOptions _emailSettings;

        public EmailSender(IOptions<SmtpOptions> emailOptions)
        {
            _emailSettings = emailOptions.Value;
        }

        public async Task SendEmailAsync(string account, string subject, string mess)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.EmailSender, _emailSettings.SendEmail));
            message.To.Add(new MailboxAddress(account, account));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = mess
            };

            var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => _emailSettings.UseDefaultCredentials;
            client.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.EnableSsl);

            // Note: only needed if the SMTP server requires authentication
            client.Authenticate(_emailSettings.SendEmail, _emailSettings.AuthorizationCode);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}