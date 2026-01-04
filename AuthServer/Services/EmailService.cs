using MailKit.Net.Smtp;
using MimeKit;

namespace AuthServer.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendTestEmail(string recipientName, string recipientEmailAddress)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_configuration["Email:SenderName"], _configuration["Email:SenderEmailAddress"]));
            email.To.Add(new MailboxAddress(recipientName, recipientEmailAddress));

            email.Subject = "Test Email";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "<p>Hello world</p>"
            };

            SendEmailViaSmtp(email);
        }

        private void SendEmailViaSmtp(MimeMessage email)
        {
            using (var smtp = new SmtpClient())
            {
                // Connect to SMTP server
                smtp.Connect(_configuration["Email:SmtpHost"], _configuration.GetValue<int>("Email:SmtpPort"));
                smtp.Authenticate(_configuration["Email:SmtpUsername"], _configuration["Email:SmtpPassword"]);

                // Send email
                smtp.Send(email);

                // Disconnect
                smtp.Disconnect(true);
            }
        }
    }
}
