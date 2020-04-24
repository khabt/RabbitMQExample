using RabbitMQExample.Implements;
using RabbitMQExample.Interfaces;
using RabbitMQExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample
{
    class SendMail
    {
        private IEmailProcessor EmailProcessor;
        private void ProcessEmailItem(object args)
        {
            QueueItem item = (QueueItem)args;
            string msg = item.Message;
            EmailProcessor.ProcessEmail(msg);
        }
    }
}
