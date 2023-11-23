namespace RabbitMQ_API.Model
{
    public class ResultMessage<T>
    {
        public int code { get; set; }
        public string? message { get; set; }
        public T? data { get; set; }
    }
}
