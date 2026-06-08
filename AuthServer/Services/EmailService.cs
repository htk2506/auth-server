using AuthServer.Database.Models;
using AuthServer.Helpers;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.Json;

namespace AuthServer.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Sends a user an email with a token for resetting their password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Task SendPasswordResetTokenEmail(AppUser user, string token)
        {
            // Make sure user has an email
            if (user.Email == null) { throw new NullReferenceException("User doesn't email address."); }

            // Construct email
            var email = new MimeMessage();

            // Set sender and recipient
            email.From.Add(new MailboxAddress(_configuration["Email:SenderName"], _configuration["Email:SenderEmailAddress"]));
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
            using (var smtp = new SmtpClient())
            {
                string smtpHost = _configuration.GetValue<string>("Email:SmtpHost") ?? throw new NullReferenceException("Missing Email:SmtpHost.");
                int smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
                string smtpUsername = _configuration.GetValue<string>("Email:SmtpUsername") ?? throw new NullReferenceException("Missing Email:SmtpUsername.");
                string smtpPassword = _configuration.GetValue<string>("Email:SmtpPassword") ?? throw new NullReferenceException("Missing Email:SmtpPassword.");

                // Connect to SMTP server
                await smtp.ConnectAsync(smtpHost, smtpPort);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);

                // Send email
                await smtp.SendAsync(email);

                // Disconnect
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
