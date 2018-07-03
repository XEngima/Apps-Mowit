using System;
using System.Collections.Generic;
using System.Text;

namespace Mowit
{
    public class EmailConfig
    {
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
