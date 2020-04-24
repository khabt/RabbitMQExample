using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Models
{
    class SmtpConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; } = true;

        public SmtpConfig(string host, int port, string userName, bool enableSSL)
        {
            Host = host;
            Port = port;
            UserName = userName;
            EnableSSL = enableSSL;
        }
    }
}
