using Microsoft.AspNetCore.Identity.UI.Services;
using Sanabel.Web.Implementation;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Sanabel.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:UserName"], _config["Smtp:Password"]),
                EnableSsl = bool.Parse(_config["Smtp:EnableSSL"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:UserName"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
