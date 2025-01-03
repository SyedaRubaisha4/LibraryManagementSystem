using System.Net.Mail;
using System.Net;

namespace Models.DTOModel
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly string _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;
        public EmailService(IConfiguration configuration, string host, string port, string username, string password, string fromEmail)
        {
            _configuration = configuration;
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _fromEmail = fromEmail;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_host)
            {
                Port = int.Parse(_port),
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            smtpClient.Send(mailMessage);
        }
    }


}
