using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQExample.Interfaces;
using RabbitMQExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQExample.RabbitMQ
{
    public class RabbitSendService
    {
        private static IConnection _connection = null;
        private static IModel _channel = null;
        private static IBasicProperties _properties = null;
        private string _emailQueue = null;
        private Config _config;
        private static readonly ILog _log = LogManager.GetLogger(typeof(RabbitSendService));
        private IEmailProcessor EmailProcessor;
        //private WaitCallback waitCallback;
        public static RabbitSendService Instance { get; } = new RabbitSendService();

        private void Init()
        {
            if (_config != null)
                Init(_config.uri, _config.emailQueue, _config.durable);
        }
        public void Init(string uri, string emailQueue, bool durable = true)
        {
            try
            {
                var _connectionFactory = new ConnectionFactory
                {
                    Uri = new Uri(uri),
                    RequestedConnectionTimeout = TimeSpan.FromMilliseconds(5000)
                };
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                this._emailQueue = emailQueue;
                _channel.QueueDeclare(queue: _emailQueue,
                    durable: durable,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _properties = _channel.CreateBasicProperties();
                _properties.Persistent = durable;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        public void Shutdown()
        {
            if (_channel != null && _channel.IsOpen)
                _channel.Dispose();
            if (_connection != null && _connection.IsOpen)
                _connection.Dispose();
        }

        public void EnqueueEmail(Email email)
        {
            string message = JsonConvert.SerializeObject(email);
            this.EnqueueMessage(message, this._emailQueue);
        }

        private void EnqueueMessage(string message, string queueName)
        {
            try
            {
                if (string.IsNullOrEmpty(queueName))
                    throw new ArgumentException("Querue name is empty");
                if (_channel == null)
                    throw new Exception("Connection channel is null");

                byte[] body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: string.Empty,
                    routingKey: queueName,
                    basicProperties: _properties,
                    body: body);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                RestartService();
            }
        }
        private void RestartService()
        {
            Shutdown();
            Init();
        }

        internal class Config
        {
            public string uri;
            public string emailQueue;
            public bool durable;
            public Config(string uri, string emailQueue, bool durable)
            {
                this.uri = uri;
                this.emailQueue = emailQueue;
                this.durable = durable;
            }
        }

        private void ListenQueue(WaitCallback waitCallback)
        {
            _channel.QueueDeclare(queue: _emailQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: (ushort)10, global: false);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, evenInfo) =>
            {
                byte[] body = evenInfo.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                ThreadPool.QueueUserWorkItem(waitCallback, new QueueItem()
                {
                    Message = message,
                    EventInfo = evenInfo
                });
                await Task.Yield();
            };
        }
        private void ProcessEmailItem(object args)
        {
            QueueItem item = (QueueItem)args;
            string msg = item.Message;
            EmailProcessor.ProcessEmail(msg);
        }

        public void Listen()
        {
            ListenQueue(ProcessEmailItem);
        }
    }
}
