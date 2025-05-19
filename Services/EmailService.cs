using System.Net.Mail;
using System.Net;

namespace SarawakTourismApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            var portString = _configuration["Email:Port"];
            if (!int.TryParse(portString, out int port))
                throw new InvalidOperationException("Email:Port configuration is missing or invalid.");

            var fromAddress = _configuration["Email:From"];
            if (string.IsNullOrEmpty(fromAddress))
                throw new InvalidOperationException("Email:From configuration is missing.");

            using var client = new SmtpClient(_configuration["Email:SmtpServer"])
            {
                Port = port,
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                ),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = "Password Reset Request",
                Body = $@"
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password. Click the link below to proceed:</p>
                    <p><a href='{resetLink}'>{resetLink}</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request this, please ignore this email.</p>",
                IsBodyHtml = true
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
} 