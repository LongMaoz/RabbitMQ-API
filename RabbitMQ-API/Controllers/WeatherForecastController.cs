using Microsoft.AspNetCore.Mvc;
using RabbitMQService.RabbitMQ;
using RabbitMqHttpApiClient;
using RabbitMqHttpApiClient.API;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ_API.Model;

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
        const string ExchangerName = "���Խ�����";
        const string QueueName = "���Զ���";
        public static object _lock = new object();
        public WeatherForecastController(IRMQQueueDeclarePublish publish, IRMQQueueDeclareSubscribe subscribe, ILogger<WeatherForecastController> logger, IRMQPublishHanlderDelegate rMQPublishHanlder)
        {
            this._subscribe = subscribe;
            _logger = logger;
            _hanlder = rMQPublishHanlder;
            _publish = publish;
        }

        [HttpPost]
        public async Task<ResultMessage<bool>> SendMessage([FromForm]string msg)
        {
            await _publish.RabbitMQExchangerRoutingKeydirectPublishAsync(ExchangerName, QueueName, msg, "Jackpot",true);
            return new ResultMessage<bool>
            {
                code = 200,
                data = true,
                message = "OK",
            };
        }

        [HttpPost]
        public async Task<ResultMessage<bool>> SendMessage_([FromForm] string msg, [FromForm] string exchangerName,[FromForm] string queueName, [FromForm] string routerKey, [FromForm] bool durable)
        {
            await _publish.RabbitMQExchangerRoutingKeydirectPublishAsync(exchangerName, queueName, msg, routerKey, durable);
            return new ResultMessage<bool>
            {
                code = 200,
                data = true,
                message = "OK",
            };
        }

        /// <summary>
        /// ����mq��Ϣ������
        /// </summary>
        [HttpGet]
        public async Task<ResultMessage<bool>> StartHanlder()
        {
            _hanlder.ReceiveMessageCallback = x =>
            {
                string str = "��Ϣ���д�������:������Ϊ"+ x.ConsumerTag;
                string path = @"E:\Work\test.txt";
                
                lock (_lock)
                {
                    Random rand = new Random();
                    int randomNumber = rand.Next(); 
                    Console.WriteLine(randomNumber);
                    randomNumber = rand.Next(1, 11);
                    if (randomNumber>5)
                    {
                        throw new Exception();
                    }
                    using (StreamWriter writer = new StreamWriter(path, true)) // �ڶ�������Ϊtrue��ʾ׷��ģʽ
                    {
                        writer.WriteLine(str); // WriteLine���Զ����ı�����������з�
                    }
                }
                return Task.FromResult(true);

            };
            await _subscribe.RabbitMQExchangerRoutingKeydirectSubscribeAsync(ExchangerName,QueueName,"Jackpot",true);
            return new ResultMessage<bool>
            {
                code = 200,
                data = true,
                message = "OK",
            };
        }

        [HttpGet]
        public async void StopHanlder(int channelNumber, [FromForm] string consumer_tag)
        {
            await _subscribe.StopConsumers(channelNumber, consumer_tag, ExchangerName, QueueName, "Jackpot",true);
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