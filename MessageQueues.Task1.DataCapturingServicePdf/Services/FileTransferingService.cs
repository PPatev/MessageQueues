using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace MessageQueues.Task1.DataCapturingServicePdf.Services
{
    public class FileTransferingService : ITransferingService
    {
        private readonly FileTransferConfig _fileTransferConfig;
        private readonly IMessageProducer _messageProducer;
        private readonly int chunkSize = 4096;

        public FileTransferingService(IOptions<FileTransferConfig> fileTransferOptions, IMessageProducer messageProducer)
        {
            _fileTransferConfig = fileTransferOptions.Value;
            _messageProducer = messageProducer;
        }

        public void StartTransfering()
        {
            
            var dataDirectory = @$"{_fileTransferConfig.DataDirectory}\";
            var currentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);
            var fileExtensions = $"*.{_fileTransferConfig.FileType}";

            var dataFiles = Directory.GetFiles(workingDirectory, fileExtensions);

            foreach (var filePath in dataFiles)
            {
                var fileName = Path.GetFileName(filePath);

                Console.WriteLine($"Press Enter to start transfering file {fileName}");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                { 
                    SendFile(filePath);
                    Console.WriteLine($"File {fileName} sent succesfully");
                }
            }

                //foreach (var filePath in dataFiles)
                //{
                //    // transfer to queue
                //    //var pathNew = @"newfile.pdf";
                //    //var newFilePath = Path.Combine(workingDirectory, pathNew);

                //    using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 20, true))
                //    {
                //        // Read the source file into a byte array.
                //        byte[] bytes = new byte[reader.Length];
                //        int numBytesToRead = (int)reader.Length;
                //        int numBytesRead = 0;
                //        while (numBytesToRead > 0)
                //        {
                //            // Read may return anything from 0 to numBytesToRead.
                //            int n = reader.Read(bytes, numBytesRead, numBytesToRead);

                //            // Break when the end of the file is reached.
                //            if (n == 0)
                //                break;

                //            numBytesRead += n;
                //            numBytesToRead -= n;
                //        }


                //        //channel.BasicPublish("fileTransfer", "", null, bytes);
                //        numBytesToRead = bytes.Length;




                //        _messageProducer.SendBytes(headers, new List<byte[]> { bytes });

                //        // Write the byte array to the other FileStream.
                //        //using (FileStream fsNew = new FileStream(newFilePath,
                //        //    FileMode.Create, FileAccess.Write))
                //        //{
                //        //    fsNew.Write(bytes, 0, numBytesToRead);
                //        //}
                //    }
                //}
        }

        private void SendFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            Console.WriteLine("Starting file read operation...");

            FileStream fileStream = File.OpenRead(filePath);
            StreamReader streamReader = new StreamReader(fileStream);
            int remainingFileSize = Convert.ToInt32(fileStream.Length);
            //int totalFileSize = Convert.ToInt32(fileStream.Length);
            //bool finished = false;

            var bytes = new List<byte[]>();

            byte[] buffer;
            while (true)
            {
                if (remainingFileSize <= 0) break;
                int read = 0;
                if (remainingFileSize > chunkSize)
                {
                    buffer = new byte[chunkSize];
                    read = fileStream.Read(buffer, 0, chunkSize);
                    bytes.Add(buffer);
                }
                else
                {
                    buffer = new byte[remainingFileSize];
                    read = fileStream.Read(buffer, 0, remainingFileSize);
                    bytes.Add(buffer);
                    //finished = true;
                }

                //IBasicProperties basicProperties = model.CreateBasicProperties();
                //basicProperties.SetPersistent(true);
                //basicProperties.Headers = new Dictionary<string, object>();
                //basicProperties.Headers.Add("output-file", chunkName);
                //basicProperties.Headers.Add("finished", finished);

                //model.BasicPublish("", RabbitMqService.ChunkedMessageBufferedQueue, basicProperties, buffer);
                remainingFileSize -= read;
            }

            _messageProducer.SendBytes(fileName, bytes);
        }
    }
}
