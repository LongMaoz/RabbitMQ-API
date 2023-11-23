using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Newtonsoft.Json;
using Polly.Retry;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.RabbitMQ
{
    /// <summary>
    /// 定义rmq生产者
    /// </summary>
    public class RMQQueueDeclarePublish : IRMQQueueDeclarePublish
    {
        private RMqConnectionFactory factory { get; }

        public RMQQueueDeclarePublish()
        {
            factory = RMqConnectionFactory.ConnFactory.GetConnectionFactory();
        }

        /// <summary>
        /// 创建信道，走默认交换机
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="dataMsg"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public async Task RabbitMQDefaultPublishAsync(string queueName, object dataMsg, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            using (var _channel = factory.GetModel())
            {
                var str = JsonConvert.SerializeObject(dataMsg);
                var bt = Encoding.Default.GetBytes(str);

                _channel.QueueDeclare(queue: queueName, durable: durable,
                    exclusive: exclusive,
                    autoDelete: autoDelete,
                    arguments: arguments);

                _channel.BasicPublish("", queueName, null, bt);
            }
        }

        /// <summary>
        /// 自定义交换机(Fanout)模式
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="dataMsg"></param>
        /// <returns></returns>
        public async Task RabbitMQExchangerDefaultRoutingKeyFanoutPublishAsync(string exchangeName, string queueName, object dataMsg, bool durable = false)
        {
            using (var _channel = factory.GetModel())
            {
                var str = JsonConvert.SerializeObject(dataMsg);
                var bt = Encoding.Default.GetBytes(str);

                _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout", durable: durable);
                _channel.QueueDeclare(queue: queueName, durable: durable, false, false, null);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");
                _channel.BasicPublish(exchange: exchangeName, "", null, bt);
            }
        }

        /// <summary>
        /// 自定义交换机（direct）模式
        /// </summary>
        /// <returns></returns>
        public async Task RabbitMQExchangerRoutingKeydirectPublishAsync(string exchangeName, string queueName, object dataMsg, string RoutingKey, bool durable = false)
        {
            using (var _channel = factory.GetModel())
            {
                var str = JsonConvert.SerializeObject(dataMsg);
                var bt = Encoding.Default.GetBytes(str);

                //创建公共direct模式死信队列
                var dlxExchangeName = $"deadDirect_{exchangeName}";
                _channel.ExchangeDeclare(exchange: dlxExchangeName, type: "direct", durable: durable);
                _channel.QueueDeclare(queue: $"deadDirect_{queueName}", durable: durable, false, false, null);
                _channel.QueueBind(queue: $"deadDirect_{queueName}", exchange: dlxExchangeName, routingKey: $"deadDirect_{RoutingKey}");

                var arguments = new Dictionary<string, object>()
                {
                    //{ "x-message-ttl",10000 },
                    { "x-dead-letter-exchange",dlxExchangeName },
                    { "x-dead-letter-routing-key", $"deadDirect_{RoutingKey}" }
                };
                //消息是否持久化？
                IBasicProperties basic = _channel.CreateBasicProperties();
                basic.DeliveryMode = (byte)(durable ? 2 : 1);
                _channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: durable);
                _channel.QueueDeclare(queue: queueName, durable: durable, false, false, arguments);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: RoutingKey);
                _channel.BasicPublish(exchange: exchangeName, RoutingKey, basic, bt);
                _channel.Close();
            }
        }

        /// <summary>
        /// 自定义交换机（TPOIC订阅）模式
        /// </summary>
        /// <returns></returns>
        public async Task RabbitMQExchangerRoutingKeytopicPublishAsync(string exchangeName, string queueName, object dataMsg, string RoutingKey, bool durable = false)
        {
            using (var _channel = factory.GetModel())
            {
                var proper = _channel.CreateBasicProperties();
                var str = JsonConvert.SerializeObject(dataMsg);
                var bt = Encoding.Default.GetBytes(str);

                _channel.ExchangeDeclare(exchange: exchangeName, type: "topic", durable: durable);
                _channel.QueueDeclare(queue: queueName, durable: durable, false, false, null);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: RoutingKey);
                _channel.BasicPublish(exchangeName, RoutingKey, true, proper, bt);
            }
        }
    }
}
