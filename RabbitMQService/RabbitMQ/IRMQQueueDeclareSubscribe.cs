using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.RabbitMQ
{
    public interface IRMQQueueDeclareSubscribe
    {
        Task RabbitMQDefaultSubscribeAsync(string queueName, bool autoAck = false);
        Task RabbitMQWorkQueueSubscribeAsync(string queueName, ushort prefetchCount = 1, bool autoAck = false);

        /// <summary>
        /// Direct模式
        /// 自定义交换机，提供路由
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        /// <param name="durable"></param>
        /// <param name="autoAck"></param>
        /// <param name="prefetchCount"></param>
        /// <returns></returns>
        Task RabbitMQExchangerRoutingKeydirectSubscribeAsync(string exchangeName, string queueName, string routingKey, bool durable = false, bool autoAck = false, ushort prefetchCount = 1);

        /// <summary>
        /// Fanout
        /// 自定义交换机，提供路由
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="autoAck"></param>
        /// <param name="prefetchCount"></param>
        /// <returns></returns>
        Task RabbitMQExchangerDefaultRoutingKeyFanoutSubscribeAsync(string exchangeName, string queueName, bool durable = false, bool autoAck = false, ushort prefetchCount = 1);

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
        Task RabbitMQExchangerRoutingKeytopicSubscribeAsync(string exchangeName, string queueName,string routingKey, bool durable = false, bool autoAck = false, ushort prefetchCount = 1);

        /// <summary>
        /// 停止消费者
        /// </summary>
        /// <param name="consumers_tag"></param>
        /// <returns></returns>
        Task StopConsumers(int channelNumber, string consumers_tag, string exchangeName, string queueName, string routingKey, bool durable = false);
    }
}
