using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace RabbitMQService.RabbitMQ
{
    /// <summary>
    /// RMQ消费类
    /// </summary>
    public class RMQQueueDeclareSubscribe : IRMQQueueDeclareSubscribe
    {
        private RMqConnectionFactory _factory;

        IRMQPublishHanlderDelegate _hanlder;

        public RMQQueueDeclareSubscribe(IRMQPublishHanlderDelegate hanlder)
        {
            _factory = RMqConnectionFactory.ConnFactory.GetConnectionFactory();
            _hanlder = hanlder;
        }

        /// <summary>
        /// Hello World消费模式
        /// </summary>
        /// <param name="queueName">需要消费的信道名称</param>
        /// <param name="autoAck">确认类型确认|true:自动确认|false:手动确认</param>
        /// <returns></returns>
        public async Task RabbitMQDefaultSubscribeAsync(string queueName, bool autoAck = false)
        {
            var _channel = _factory.GetModel();
            {
                _channel.QueueDeclare(queue: queueName, false, false, false, null);
                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        await _hanlder.PublishHanlderAsync(ea);
                        if (!autoAck)
                            _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch
                    {
                        if (!autoAck)
                            _channel.BasicNack(ea.DeliveryTag, true, true);
                    }
                };
                _channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            }
        }


        /// <summary>
        /// WorkQueue消费模式（能者多劳模式）
        /// 如果这台服务器配置好，能一次性处理多次，可以将prefetchCount加大参数数值
        /// </summary>
        /// <param name="queueName">需要消费的信道名称</param>
        /// <param name="autoAck">确认类型确认|true:自动确认|false:手动确认</param>
        /// <param name="prefetchCount">当前消费者处理的数量，默认1</param>
        /// <returns></returns>
        public async Task RabbitMQWorkQueueSubscribeAsync(string queueName, ushort prefetchCount = 1, bool autoAck = false)
        {
            var _channel = _factory.GetModel();
            {
                _channel.QueueDeclare(queue: queueName, false, false, false, null);
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {

                        await _hanlder.PublishHanlderAsync(ea);
                        if (!autoAck)
                            _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch
                    {
                        if (!autoAck)
                            _channel.BasicNack(ea.DeliveryTag, true, true);
                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            }
        }

        /// <summary>
        /// Fanout模式
        /// 自定义交换机，但不提供路由
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="autoAck"></param>
        /// <param name="prefetchCount"></param>
        /// <returns></returns>
        public async Task RabbitMQExchangerDefaultRoutingKeyFanoutSubscribeAsync(string exchangeName, string queueName, bool durable = false, bool autoAck = false, ushort prefetchCount = 1)
        {
            var _channel = _factory.GetModel();
            {
                _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout", durable: durable);
                _channel.QueueDeclare(queue: queueName, false, false, false, null);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {

                    try
                    {

                        await _hanlder.PublishHanlderAsync(ea);
                        if (!autoAck)
                            _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch
                    {
                        if (!autoAck)
                            _channel.BasicNack(ea.DeliveryTag, true, true);
                    }

                };

                _channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            }
        }

        /// <summary>
        /// Direct
        /// 自定义交换机，提供路由
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        /// <param name="durable"></param>
        /// <param name="autoAck"></param>
        /// <param name="prefetchCount"></param>
        /// <returns></returns>
        public async Task RabbitMQExchangerRoutingKeydirectSubscribeAsync(string exchangeName, string queueName, string routingKey, bool durable = false, bool autoAck = false, ushort prefetchCount = 1)
        {
            var _channel = _factory.GetModel();
            {
                //创建公共direct模式死信队列
                var dlxExchangeName = $"deadDirect_{exchangeName}";
                _channel.ExchangeDeclare(exchange: dlxExchangeName, type: "direct", durable: durable);
                _channel.QueueDeclare(queue: $"deadDirect_{queueName}", durable: durable, false, false, null);
                _channel.QueueBind(queue: $"deadDirect_{queueName}", exchange: dlxExchangeName, routingKey: $"deadDirect_{routingKey}");

                var arguments = new Dictionary<string, object>()
                {
                    //{ "x-message-ttl",10000 },
                    { "x-dead-letter-exchange",dlxExchangeName },
                    { "x-dead-letter-routing-key", $"deadDirect_{routingKey}" }
                };

                _channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: durable);
                _channel.QueueDeclare(queue: queueName, false, false, false, arguments);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        await _hanlder.PublishHanlderAsync(ea);

                        if (!autoAck)
                            _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        if (!autoAck)
                            _channel.BasicNack(ea.DeliveryTag, true, true);
                    }
                };
                _channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            }
        }




        /// <summary>
        /// Topic模式
        /// 自定义交换机，提供模糊匹配路由
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="autoAck"></param>
        /// <param name="prefetchCount"></param>
        /// <returns></returns>
        public async Task RabbitMQExchangerRoutingKeytopicSubscribeAsync(string exchangeName, string queueName, string routingKey, bool durable = false, bool autoAck = false, ushort prefetchCount = 1)
        {
            var _channel = _factory.GetModel();
            {
                _channel.ExchangeDeclare(exchange: exchangeName, type: "topic", durable: durable);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        await _hanlder.PublishHanlderAsync(ea);
                        if (!autoAck)
                            _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch
                    {
                        if (!autoAck)
                            _channel.BasicNack(ea.DeliveryTag, true, true);
                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            }

        }

        public Task StopConsumers(int channelNumber, string consumers_tag,string exchangeName, string queueName, string routingKey, bool durable = false)
        {
            var channers = _factory.GetChanners();
            {
                IModel channer = channers.Find(x => x.ChannelNumber == channelNumber);
                channer.BasicCancel(consumers_tag);
                channer.Close();
            }
            return Task.CompletedTask;
        }
    }
}
