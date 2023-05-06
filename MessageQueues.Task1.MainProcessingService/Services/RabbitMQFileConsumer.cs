using MessageQueues.Task1.MainProcessingService.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using MessageQueues.Task1.MainProcessingService.Interfaces;

namespace MessageQueues.Task1.MainProcessingService.Services
{
    public class RabbitMQFileConsumer : IMessageConsumer
    {
        private readonly RabbitMQConfig _rabbitMQConfig;

        public RabbitMQFileConsumer(IOptions<RabbitMQConfig> rabbitMQOptions)
        {
            _rabbitMQConfig = rabbitMQOptions.Value;
        }

        public void StartConsuming(string outputDirectory)
        {
            var factory = new ConnectionFactory();
            factory.DispatchConsumersAsync = true;
            factory.Uri = new Uri(_rabbitMQConfig.ConnectionString);

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueBind(
                       queue: _rabbitMQConfig.QueueName,
                       exchange: _rabbitMQConfig.ExchangeName,
                       routingKey: string.Empty,
                       arguments: _rabbitMQConfig.Headers);

                    var consumer = new AsyncEventingBasicConsumer(channel);
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

                    channel.BasicConsume("fileTransferQueue", true, consumer);
                    Console.ReadKey();
                }
            }
        }
    }
}
