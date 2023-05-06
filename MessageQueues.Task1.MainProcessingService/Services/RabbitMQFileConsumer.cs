using MessageQueues.Task1.MainProcessingService.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using MessageQueues.Task1.MainProcessingService.Interfaces;

namespace MessageQueues.Task1.MainProcessingService.Services
{
    public class RabbitMqFileConsumer : IMessageConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqConfig _rabbitMQConfig;

        public RabbitMqFileConsumer(IConnection connection, IOptions<RabbitMqConfig> rabbitMQOptions)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _rabbitMQConfig = rabbitMQOptions.Value;
        }

        public void StartConsuming(string outputDirectory)
        {
            _channel.QueueBind(
                       queue: _rabbitMQConfig.QueueName,
                       exchange: _rabbitMQConfig.ExchangeName,
                       routingKey: string.Empty,
                       arguments: _rabbitMQConfig.Headers);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            Console.WriteLine("Waiting for messages....");
            Console.WriteLine("Press any key to stop consuming messages...");

            consumer.Received += async (sender, eventArgs) =>
            {
                Console.WriteLine("Received a message!");
                var headers = eventArgs.BasicProperties.Headers;

                var chunkName = Encoding.UTF8.GetString(headers["output-file"] as byte[]);
                var isLastChunk = Convert.ToBoolean(headers["finished"]);
                var fileName = Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["fileName"] as byte[]);

                var newFilePath = Path.Combine(outputDirectory, fileName);

                using (FileStream fileStream = new FileStream(newFilePath, FileMode.Append, FileAccess.Write))
                {
                    var bytes = eventArgs.Body.ToArray();
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }

                if (isLastChunk)
                {
                    Console.WriteLine($"File {fileName} received and written to {outputDirectory}");
                }
                else
                {
                    Console.WriteLine($"Chunk saved - {chunkName}. Finished? {isLastChunk}", isLastChunk);
                }

                await Task.Yield();
            };

            _channel.BasicConsume("fileTransferQueue", true, consumer);
            Console.ReadLine();
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
