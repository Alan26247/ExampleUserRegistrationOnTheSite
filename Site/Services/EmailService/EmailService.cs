using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Site.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly string organization;
        private readonly string mailServer;
        private readonly string mailSender;
        private readonly int port;
        private readonly string login;
        private readonly string password;
        private readonly bool ssl;
        private readonly int timeout;

        public EmailService(IConfiguration configuration)
        {
            // извлечение настроек
            organization = configuration["EmailService:Organization"];
            mailServer = configuration["EmailService:MailServer"];
            mailSender = configuration["EmailService:MailSender"];
            port = int.Parse(configuration["EmailService:Port"]);
            login = configuration["EmailService:Login"];
            password = configuration["EmailService:Password"];
            ssl = bool.Parse(configuration["EmailService:SSL"]);
            timeout = int.Parse(configuration["EmailService:Timeout"]);
        }

        // отправить сообщение
        public void SendEmailAsync(string email, string subject, string body)
        {
            MailMessage message = new()
            {
                // отправитель - устанавливаем адрес и отображаемое в письме название организации
                From = new MailAddress(mailSender, organization)
            };
            // кому отправляем
            message.To.Add(email);
            // тема письма
            message.Subject = subject;
            // текст письма
            message.Body = body;
            // письмо представляет код html
            message.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            using SmtpClient smtp = new(mailServer);
            // логин и пароль
            smtp.Credentials = new NetworkCredential(login, password);
            smtp.Port = port;
            smtp.EnableSsl = ssl;
            smtp.Timeout = timeout;
            smtp.Send(message);
        }
    }
}
