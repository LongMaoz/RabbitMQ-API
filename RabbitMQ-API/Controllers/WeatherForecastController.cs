using Microsoft.AspNetCore.Mvc;
using RabbitMQService.RabbitMQ;
using RabbitMqHttpApiClient;
using RabbitMqHttpApiClient.API;
using Newtonsoft.Json;

namespace CenterMQ.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IRMQQueueDeclareSubscribe _subscribe;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRMQPublishHanlderDelegate _hanlder;
        private readonly IRMQQueueDeclarePublish _publish;
        const string ExchangerName = "测试交换机";
        const string QueueName = "测试队列";
        public WeatherForecastController(IRMQQueueDeclarePublish publish, IRMQQueueDeclareSubscribe subscribe, ILogger<WeatherForecastController> logger, IRMQPublishHanlderDelegate rMQPublishHanlder)
        {
            this._subscribe = subscribe;
            _logger = logger;
            _hanlder = rMQPublishHanlder;
            _publish = publish;
        }

        [HttpPost]
        public async void SendMessage(string msg)
        {
            await _publish.RabbitMQExchangerRoutingKeydirectPublishAsync(ExchangerName, QueueName, msg, "Jackpot");
        }

        /// <summary>
        /// 启动mq消息拦截器
        /// </summary>
        [HttpGet]
        public async void StartHanlder()
        {
            _hanlder.ReceiveMessageCallback = x =>
            {
                return Task.CompletedTask;
            };
            await _subscribe.RabbitMQExchangerRoutingKeydirectSubscribeAsync(ExchangerName, QueueName, "Jackpot");
        }

        [HttpGet]
        public async void StopHanlder(int channelNumber, string consumer_tag)
        {
            await _subscribe.StopConsumers(channelNumber, consumer_tag, ExchangerName, QueueName, "Jackpot");
        }

        [HttpGet]
        public async Task<string> GetOverview()
        {
            RabbitMqApi api = new RabbitMqApi("http://localhost:15672", "guest", "guest");
            var result = await api.GetOverview();
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public async Task<string> GetConsumers()
        {
            RabbitMqApi api = new RabbitMqApi("http://localhost:15672", "guest", "guest");
            var result = await api.GetConsumers();
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public async Task<string> GetExchanges()
        {
            RabbitMqApi api = new RabbitMqApi("http://localhost:15672", "guest", "guest");
            var result = await api.GetExchanges();
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public async Task<string> GetChannels()
        {
            RabbitMqApi api = new RabbitMqApi("http://localhost:15672", "guest", "guest");
            var result = await api.GetChannels();
            return JsonConvert.SerializeObject(result);
        }
    }
}