using MessageQueues.Task1.MainProcessingService.Helpers;
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
            var workingDirectory = DirectoryHelper.GetWorkingDirectory(AppContext.BaseDirectory, _fileProcessingConfig.OutputDirectory);
            
            _messageConsumer.StartConsuming(workingDirectory);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
