using MessageQueues.Task1.DataCapturingServicePdf.Helpers;
using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using Microsoft.Extensions.Options;

namespace MessageQueues.Task1.DataCapturingServicePdf.Services
{
    public class FileTransferingService : ITransferingService
    {
        private readonly FileTransferConfig _fileTransferConfig;
        private readonly IMessageProducer _messageProducer;
        
        public FileTransferingService(IOptions<FileTransferConfig> fileTransferOptions, IMessageProducer messageProducer)
        {
            _fileTransferConfig = fileTransferOptions.Value;
            _messageProducer = messageProducer;
        }

        public void StartTransfering()
        {
            var dataFiles = FileHelper.GetFilesFromDirectory(AppContext.BaseDirectory, _fileTransferConfig.DataDirectory, _fileTransferConfig.FileType);

            foreach (var filePath in dataFiles)
            {
                var fileName = Path.GetFileName(filePath);

                Console.WriteLine($"Press Enter to start transfering file {fileName}");
                Console.ReadLine();

                SendFile(filePath);
                Console.WriteLine($"File {fileName} sent succesfully");
            }
        }

        private void SendFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            Console.WriteLine("Starting file read operation...");

            var fileStream = File.OpenRead(filePath);
            int remainingFileSize = Convert.ToInt32(fileStream.Length);
            
            var bytes = new List<byte[]>();

            byte[] buffer;
            while (true)
            {
                if (remainingFileSize <= 0) break;
                int read = 0;
                if (remainingFileSize > _fileTransferConfig.ChunkSize)
                {
                    buffer = new byte[_fileTransferConfig.ChunkSize];
                    read = fileStream.Read(buffer, 0, _fileTransferConfig.ChunkSize);
                    bytes.Add(buffer);
                }
                else
                {
                    buffer = new byte[remainingFileSize];
                    read = fileStream.Read(buffer, 0, remainingFileSize);
                    bytes.Add(buffer);
                }
                
                remainingFileSize -= read;
            }

            _messageProducer.SendBytes(fileName, _fileTransferConfig.FileType, bytes);
        }
    }
}
