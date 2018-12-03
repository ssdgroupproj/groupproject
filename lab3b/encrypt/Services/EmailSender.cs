using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace encrypt.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message, string sender)
        {
            return Execute(Options.GmailUsername, Options.GmailPassword, sender, subject, message, email);
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.GmailUsername, Options.GmailPassword, null, subject, message, email);
        }

        private Task Execute(string username, string password, string sender, string subject, string message, string email)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);

            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(username);
            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            
            client.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}