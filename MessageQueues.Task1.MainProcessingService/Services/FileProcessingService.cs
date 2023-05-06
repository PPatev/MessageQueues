using MessageQueues.Task1.MainProcessingService.Interfaces;
using MessageQueues.Task1.MainProcessingService.Models;
using Microsoft.Extensions.Options;

namespace MessageQueues.Task1.MainProcessingService.Services
{
    public class FileProcessingService : IProcessingService
    {
        private readonly FileProcessingConfig _fileProcessingConfig;
        private readonly IMessageConsumer _messageConsumer;

        public FileProcessingService(IOptions<FileProcessingConfig> fileProcessingOptions, IMessageConsumer messageConsumer)
        {
            _fileProcessingConfig = fileProcessingOptions.Value;
            _messageConsumer = messageConsumer;
        }

        public void StartProcessing() 
        {
            var dataDirectory = $@"{_fileProcessingConfig.OutputDirectory}\";
            var currentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);
            
            _messageConsumer.StartConsuming(workingDirectory);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
