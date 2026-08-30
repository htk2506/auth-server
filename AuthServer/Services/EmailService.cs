using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.Json;

namespace AuthServer.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPasswordResetTokenEmail(AppUser user, string token)
        {
            _logger.LogTrace("Start of {@FunctionName}.", nameof(SendPasswordResetTokenEmail));

            // Make sure user has an email
            if (user.Email == null) { throw new InvalidOperationException("User doesn't email address."); }

            // Construct email
            var email = new MimeMessage();

            // Set sender and recipient
            string emailSenderName = _configuration.GetValue<string>("Email:SenderName") ?? throw new InvalidOperationException("Missing Email:SenderName.");
            string emailSenderEmailAddress = _configuration.GetValue<string>("Email:SenderEmailAddress") ?? throw new InvalidOperationException("Missing Email:SenderEmailAddress.");
            email.From.Add(new MailboxAddress(emailSenderName, emailSenderEmailAddress));
            email.To.Add(new MailboxAddress(user.Username, user.Email));

            // Set the subject and body
            email.Subject = "Password Reset Token";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = JsonSerializer.Serialize(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    PasswordResetToken = token,
                    Message = $"Token expires in {_configuration.GetValue<int>("PasswordResetToken:Minutes")} minutes."
                }, Utils.DefaultJsonSerializerOptions)
            };

            await SendEmailViaSmtp(email);
        }

        /// <summary>
        /// Sends an email using an SMTP server.
        /// </summary>
        /// <param name="email">The email to send.</param>
        private async Task SendEmailViaSmtp(MimeMessage email)
        {
            _logger.LogTrace("Start of {@FunctionName}.", nameof(SendEmailViaSmtp));

            using (var smtp = new SmtpClient())
            {
                string smtpHost = _configuration.GetValue<string>("Email:SmtpHost") ?? throw new InvalidOperationException("Missing Email:SmtpHost.");
                int smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
                string smtpUsername = _configuration.GetValue<string>("Email:SmtpUsername") ?? throw new InvalidOperationException("Missing Email:SmtpUsername.");
                string smtpPassword = _configuration.GetValue<string>("Email:SmtpPassword") ?? throw new InvalidOperationException("Missing Email:SmtpPassword.");

                // Connect to SMTP server
                _logger.LogTrace("Connecting to SMTP server.");
                await smtp.ConnectAsync(smtpHost, smtpPort);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);

                // Send email
                _logger.LogDebug("Sending an email.");
                await smtp.SendAsync(email);

                // Disconnect
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
