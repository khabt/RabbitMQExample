﻿using Newtonsoft.Json;
using RabbitMQ.Client.Exceptions;
using RabbitMQExample.Interfaces;
using RabbitMQExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Implements
{
    class EmailProcessor : IEmailProcessor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailProcessor));
        private SmtpConfig _smtpConfig = null;
        public EmailProcessor(SmtpConfig smtpConfig)
        {
            _smtpConfig = smtpConfig;
        }
        public void ProcessEmail(string msg)
        {
            try
            {
                Email messageObj = JsonConvert.DeserializeObject<Email>(msg);
                messageObj.To = messageObj.To.Trim();
                SendMail(messageObj, _smtpConfig);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void SendMail(Email email, SmtpConfig smtpConfig)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage(smtpConfig.UserName, email.To, email.Subject, email.Body);
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = smtpConfig.Host;
            smtpClient.Port = smtpConfig.Port;
            smtpClient.Credentials = new NetworkCredential(smtpConfig.UserName, smtpConfig.Password);
            smtpClient.EnableSsl = smtpConfig.EnableSSL;
            smtpClient.DeliveryFormat = SmtpDeliveryFormat.International;
        }
    }
}
