using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.RabbitMQ
{
    public interface IRMQQueueDeclarePublish
    {
        /// <summary>
        /// 默认交换机模式，点对点
        /// </summary>
        /// <param name="queueName">交换机</param>
        /// <param name="dataMsg">消息体</param>
        /// <returns></returns>
        Task RabbitMQDefaultPublishAsync(string queueName, object dataMsg, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null);

        /// <summary>
        /// Fanout扇形（扩散型）模式
        /// 不处理路由键。你只需要简单的将队列绑定到交换机上。一个发送到交换机的消息都会被转发到与该交换机绑定的所有队列上。很像子网广播，每台子网内的主机都获得了一份复制的消息。Fanout交换机转发消息是最快的。 
        /// 任何发送到Fanout Exchange的消息都会被转发到与该Exchange绑定(Binding)的所有Queue上。
        /// 如果一个队列绑定到该交换机上要求路由键 “dog”，则只有被标记为“dog”的消息才被转发，不会转发dog.puppy，也不会转发dog.guard，只会转发dog。 
        /// </summary>
        /// <param name="exchangeName">交换机</param>
        /// <param name="queueName">队列名</param>
        /// <param name="dataMsg">消息体</param>
        /// <param name="durable">是否持久化,一般不填</param>
        /// <returns></returns>
        Task RabbitMQExchangerDefaultRoutingKeyFanoutPublishAsync(string exchangeName, string queueName, object dataMsg, bool durable = false);

        /// <summary>
        /// Direct直接匹配模式
        /// 处理路由键。需要将一个队列绑定到交换机上，要求该消息与一个特定的路由键完全匹配
        /// </summary>
        /// <param name="exchangeName">交换机</param>
        /// <param name="queueName">队列名</param>
        /// <param name="dataMsg">消息体</param>
        /// <param name="RoutingKey">路由</param>
        /// <param name="durable">是否持久化,一般不填</param>
        /// <returns></returns>
        Task RabbitMQExchangerRoutingKeydirectPublishAsync(string exchangeName, string queueName, object dataMsg, string RoutingKey, bool durable = false);

        /// <summary>
        /// Topic模式（常用于广播）
        /// 自定义交换机，提供模糊匹配路由
        /// 注：Topic模式下的RoutingKey可以利用模糊匹配路由的方式给多个队列广播
        /// 同一消息,应用场景给多个队列绑定一个消息的一对多广播模式
        /// 匹配关键字 
        /// #(全文匹配)：# == test.a.b || test.# == test.a.b != test1.a.b || #.test.# == a.test.a.b != a.test1.a.b
        /// *(单字匹配)：* == test || test.* == test.a || *.test.* == a.test.a
        /// </summary>
        /// <param name="exchangeName">交换机</param>
        /// <param name="queueName">队列名</param>
        /// <param name="dataMsg">消息体</param>
        /// <param name="RoutingKey">路由</param>
        /// <param name="durable">是否持久化,一般不填</param>
        /// <returns></returns>
        Task RabbitMQExchangerRoutingKeytopicPublishAsync(string exchangeName, string queueName, object dataMsg, string RoutingKey, bool durable = false);
    }
}
