using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueues.Task1.DataCapturingServicePdf.Models
{
    public class RabbitMQConfig
    {
        public string ConnectionString { get; set; }

        public string ExchangeName { get; set; }

        public string RoutingKey { get; set; }
    }
}
