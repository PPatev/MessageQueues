﻿
namespace MessageQueues.Task1.DataCapturingServicePdf.Models
{
    public class RabbitMqConfig
    {
        public string ConnectionString { get; set; }

        public string ExchangeName { get; set; }

        public string RoutingKey { get; set; }

        public string QueueName { get; set; }
    }
}
