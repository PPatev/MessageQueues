using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueues.Task1.DataCapturingServicePdf.Services
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly RabbitMQConfig _rabbitMQConfig;

        public RabbitMQProducer(IOptions<RabbitMQConfig> rabbitMQOptions) 
        { 
            _rabbitMQConfig = rabbitMQOptions.Value;
        }

        public void SendBytes(string fileName, IList<byte[]> fileBytes)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(_rabbitMQConfig.ConnectionString);
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var props = channel.CreateBasicProperties();
            props.ContentType = "application/pdf";
            props.DeliveryMode = 2;
            
            var headers = new Dictionary<string, object>();
            headers.Add("fileName", fileName);
            headers.Add("subject", "fileTransfer");
            headers.Add("fileType", "pdf");
            headers.Add("output-file", "");
            headers.Add("finished", false);

            props.Headers = headers;

            for (int i = 0; i < fileBytes.Count; i++)
            {
                var section = fileBytes[i];
                if (i == fileBytes.Count - 1)
                {
                    props.Headers["finished"] = true;
                }

                var chunkName = $"{fileName}.{Guid.NewGuid()}";
                props.Headers["output-file"] = chunkName;

                channel.BasicPublish(_rabbitMQConfig.ExchangeName, _rabbitMQConfig.RoutingKey, props, section);
            }

            channel.Close();
            connection.Close();
        }
    }
}
