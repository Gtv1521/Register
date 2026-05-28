using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FrameworkDriver_Api.src.Models;
using System.Net;

namespace FrameworkDriver_Api.src.Services
{
    public class EmailService
    {

        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        public EmailService(IOptions<EmailModel> configuration)
        {
            _host = configuration.Value.Host;
            _user = configuration.Value.User;
            _port = configuration.Value.Port;
            _password = configuration.Value.Password;
        }

        public async Task<bool> EnviarEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            SmtpClient smtpClient = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_user, _password),
                EnableSsl = true
            };       

            // Crear el mensaje de correo
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_user),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true // Si quieres que el correo soporte HTML, pon esto a true
            };
            // Destinatario del correo
            mail.To.Add(toEmail);
            // Enviar el correo
            smtpClient.Send(mail);
            return true;     
        }
    }
}