using System;
using System.Collections.Generic;
using System.Text;

namespace Mowit
{
    public class EmailConfig
    {
        public EmailConfig()
        {
            SendEmails = false;
            FromAddress = "mymower@gmail.com";
            FromName = "Mowit";
            ToAddress = "mypersonalemail.gmail.com";
            ToName = "My Name";
            Smtp = "smtp.gmail.com";
            Port = 587;
            UseDefaultCredentials = false;
            UserName = "username";
            Password = "password";
        }

        public bool SendEmails { get; set; }

        public string FromAddress { get; set; }

        public string FromName { get; set; }

        public string ToAddress { get; set; }

        public string ToName { get; set; }

        public string Smtp { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
