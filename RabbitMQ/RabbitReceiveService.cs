using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.RabbitMQ
{
    public class RabbitReceiveService
    {
        private static IConnection _connection = null;
        private static IModel _channel = null;
        private static IBasicProperties _properties = null;
        private string _emailQueue = null;        
        private static readonly ILog _log = LogManager.GetLogger(typeof(RabbitReceiveService));
    }
}
