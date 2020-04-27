using RabbitMQExample.Implements;
using RabbitMQExample.Interfaces;
using RabbitMQExample.Models;
using RabbitMQExample.RabbitMQ;
using RabbitMQExample.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample
{
    public class SendMail
    {        
        private static string To = "buikha2011@gmail.com";
        private static string Body = Utilities.ReadTemplateFile("test.html");
        private static string Subject = "test mail";

        public static void SendListMail() 
        {
            for (int i = 0; i < 100; i++)
            {
                Email mail = new Email();
                mail.To = To;
                mail.Body = Body;
                mail.Subject = Subject;
                RabbitService.Instance.EnqueueEmail(mail);
            }
        }
    }
}
