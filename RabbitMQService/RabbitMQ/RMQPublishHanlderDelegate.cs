using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.RabbitMQ
{
    public class RMQPublishHanlderDelegate : IRMQPublishHanlderDelegate
    {

        public RMQPublishHanlderDelegate()
        {
            if (ReceiveMessageCallback == null)
            {
                ReceiveMessageCallback += x =>
                {
                    return Task.CompletedTask;
                };
            }
        }


        public Func<BasicDeliverEventArgs, Task> ReceiveMessageCallback { get; set; }

        public Task PublishHanlderAsync(BasicDeliverEventArgs args) => ReceiveMessageCallback(args);

    }
}
