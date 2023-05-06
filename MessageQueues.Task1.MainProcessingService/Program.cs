using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessageQueues.Task1.MainProcessingService
{
    public  class Program
    {
        public static void Main(string[] args)
        {
            var dataDirectory = @"DataReceived\";
            var currentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);

            var factory = new ConnectionFactory();
            factory.Uri = new Uri("****");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var headers = new Dictionary<string, object> 
            {
                { "subject", "fileTransfer" },
                { "fileType", "pdf" },
                { "x-match", "all"}
            };

            channel.QueueDeclare("fileTransferQueue", true, false, false);
            channel.QueueBind("fileTransferQueue", "fileTransferExchange", "", headers);

            var consumer = new EventingBasicConsumer(channel);

            Console.WriteLine("Waitin for messages....");

            consumer.Received += (sender, args) => 
            {
                Console.WriteLine("Received a chunk!");
                var headers = args.BasicProperties.Headers;
                var chunkName = Encoding.UTF8.GetString((headers["output-file"] as byte[]));
                var isLastChunk = Convert.ToBoolean(headers["finished"]);
                var fileName = Encoding.UTF8.GetString(args.BasicProperties.Headers["fileName"] as byte[]);
                var newFilePath = Path.Combine(workingDirectory, fileName);

                using (FileStream fileStream = new FileStream(newFilePath, FileMode.Append, FileAccess.Write))
                {
                    var bytes = args.Body.ToArray();
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }
                Console.WriteLine($"Chunk saved - {chunkName}. Finished? {isLastChunk}", isLastChunk);
                if (isLastChunk)
                {
                    Console.WriteLine($"File {fileName} received and written to {workingDirectory}");
                }

            };

            channel.BasicConsume("fileTransferQueue", true, consumer);
            
            Console.ReadKey();

            channel.Close();
            connection.Close();
        }
    }
}
