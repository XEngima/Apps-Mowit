using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace Mowit
{
    public static class EmailSender
    {
        private static EmailConfig Config { get; set; }

        public static void Init(EmailConfig config)
        {
            Config = config;
        }

        public static void SendMail(string subject, string body)
        {
            if (Config == null)
            {
                throw new InvalidOperationException("E-postskickaren måste initieras innan den används.");
            }

            var smtp = new SmtpClient
            {
                Host = Config.Smtp,
                Port = Config.Port,
                EnableSsl = Config.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = Config.UseDefaultCredentials,
                Credentials = new NetworkCredential(Config.UserName, Config.Password)
            };

            using (var message = new MailMessage(
                new MailAddress(Config.FromAddress, Config.FromName), 
                new MailAddress(Config.ToAddress, Config.ToName))
            {
                Subject = subject,
                Body = body,
            })
            {
                smtp.Send(message);
            }
        }
    }
}
