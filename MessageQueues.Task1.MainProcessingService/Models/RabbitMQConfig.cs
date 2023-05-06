
namespace MessageQueues.Task1.MainProcessingService.Models
{
    public class RabbitMQConfig
    {
        public string ConnectionString { get; set; }

        public string ExchangeName { get; set; }

        public string QueueName { get; set; }

        public IDictionary<string, object> Headers { get; set; }
    }
}
