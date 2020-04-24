using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Models
{
    class QueueItem
    {
        public string Message { get; set; }
        public BasicDeliverEventArgs EventInfo { get; set; }
    }
}
