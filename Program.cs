using log4net.Config;
using RabbitMQExample.Models;
using RabbitMQExample.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample
{
    public class Program
    {
        static void Main(string[] args)
        {
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(Environment.CurrentDirectory + "Config\\log4net.config"));
            bool log4netIsConfigured = log4net.LogManager.GetRepository().Configured;
            RabbitRunService();
            int i = 0;
            decimal j = 0;
            while (i == 0)
            {
                j++;
                if (j % 10000000 == 0)
                    Console.WriteLine(j / 10000000);

            }
        }

        static void RabbitRunService()
        {
            RabbitService.Instance.Init(ConfigurationManager.AppSettings["RabbitUri"], ConfigurationManager.AppSettings["RabbitEmailQueue"]);
            SendMail.SendListMail();
            SmtpConfig smtpConfig = new SmtpConfig("smtp.gmail.com", 587, "mailrac17052019@gmail.com", "", true);
            RabbitService.Instance.Listen(smtpConfig);
        }
    }
}
