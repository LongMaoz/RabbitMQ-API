using RabbitMQService.RabbitMQ;

namespace RabbitMQ_API.ContinuationConfig
{
    public static class MqConfigureInfo
    {
        /// <summary>
        /// reabbitMq全局注册
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionEntity"></param>
        /// <returns></returns>
        public static IServiceCollection AddMqConnectionBind(this IServiceCollection services, Action<MQConnectionEntity> connectionEntity)
        {
            RMqConnectionFactory
                .ConnFactory
                .AddMqConnectionInfo(connectionEntity)
                .Build();
            return services.AddSingleton<IRMQQueueDeclarePublish, RMQQueueDeclarePublish>() //生产者
                .AddSingleton<IRMQPublishHanlderDelegate, RMQPublishHanlderDelegate>() //拦截消费者消息接收
                .AddSingleton<IRMQQueueDeclareSubscribe, RMQQueueDeclareSubscribe>(); //消费者
        }
    }
}
