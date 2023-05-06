using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageQueues.Task1.DataCapturingServicePdf.Services
{
    public class RabbitMqFileProducer : IMessageProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqConfig _rabbitMqConfig;

        public RabbitMqFileProducer(IConnection connection, IOptions<RabbitMqConfig> rabbitMqOptions) 
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _rabbitMqConfig = rabbitMqOptions.Value;
        }

        public void SendBytes(string fileName, string fileType, IList<byte[]> fileBytes)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(fileType))
            {
                throw new ArgumentException(nameof(fileType));
            }

            if (fileBytes is null)
            {
                throw new ArgumentException(nameof(fileBytes));
            }

            Console.WriteLine($"Sending  file {(fileBytes.Count > 1 ? $"{fileName} in {fileBytes.Count} chunks" : fileName)}");

            var props = _channel.CreateBasicProperties();
            props.ContentType = $"application/octet-stream";
            props.DeliveryMode = 2;

            var headers = new Dictionary<string, object>();
            headers.Add("fileName", fileName);
            headers.Add("subject", "fileTransfer");
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

                var chunkName = $"{fileName}_{i}";
                props.Headers["output-file"] = chunkName;

                _channel.BasicPublish(_rabbitMqConfig.ExchangeName, _rabbitMqConfig.RoutingKey, props, section);
            }
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
            }

            if (_connection.IsOpen)
            { 
                _connection.Close();
            }
        }
    }
}
