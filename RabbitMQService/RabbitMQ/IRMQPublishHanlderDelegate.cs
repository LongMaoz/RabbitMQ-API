using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.RabbitMQ
{
    public interface IRMQPublishHanlderDelegate
    {
        Func<BasicDeliverEventArgs, Task> ReceiveMessageCallback { get; set; }

        Task PublishHanlderAsync(BasicDeliverEventArgs args);

    }
}
