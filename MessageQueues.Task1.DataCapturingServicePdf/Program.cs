using RabbitMQ.Client;

namespace MessageQueues.Task1.DataCapturingServicePdf
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataCaptureApp.Start();
        }

        static async Task ReadFilesAsync()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://hsgelczq:swh7gTHGiCEH9__HvZG6RnRuVbGReFbG@hawk.rmq.cloudamqp.com/hsgelczq");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("fileTransferExchange", ExchangeType.Topic, true, true);


            ///////////////////////////////////////////
            var dataDirectory = @"DataToSend\";

            var currentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);

            var dataFiles = Directory.GetFiles(workingDirectory, "*.pdf");

            //var bytes = Encoding.UTF8.GetBytes("Hello");
            //channel.BasicPublish("fileTransfer", "", null, bytes);

            //while (dataFiles.Length > 0)
            //{
            //    foreach (var filename in dataFiles)
            //    {
            //        var fileBytes = await File.ReadAllBytesAsync(filename);

            //        // transfer to queue
            //        var pathNew = @"newfile.pdf";
            //        var newFilePath = Path.Combine(workingDirectory, pathNew);


            //        using (var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //        {
            //            // Read the source file into a byte array.
            //            byte[] bytes = new byte[reader.Length];
            //            int numBytesToRead = (int)reader.Length;
            //            int numBytesRead = 0;
            //            while (numBytesToRead > 0)
            //            {
            //                // Read may return anything from 0 to numBytesToRead.
            //                int n = reader.Read(bytes, numBytesRead, numBytesToRead);
            //                channel.BasicPublish("fileTransfer", "", false, null);

            //                // Break when the end of the file is reached.
            //                if (n == 0)
            //                    break;

            //                numBytesRead += n;
            //                numBytesToRead -= n;
            //            }
            //            numBytesToRead = bytes.Length;

            //            // Write the byte array to the other FileStream.
            //            using (FileStream fsNew = new FileStream(newFilePath,
            //                FileMode.Create, FileAccess.Write))
            //            {
            //                fsNew.Write(bytes, 0, numBytesToRead);
            //            }
            //        }
            //    }
            //}

            foreach (var filename in dataFiles)
            {
                var fileBytes = await File.ReadAllBytesAsync(filename);

                // transfer to queue
                var pathNew = @"newfile.pdf";
                var newFilePath = Path.Combine(workingDirectory, pathNew);


                using (var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    // Read the source file into a byte array.
                    byte[] bytes = new byte[reader.Length];
                    int numBytesToRead = (int)reader.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = reader.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }


                    channel.BasicPublish("fileTransfer", "", null, bytes);
                    numBytesToRead = bytes.Length;

                    // Write the byte array to the other FileStream.
                    //using (FileStream fsNew = new FileStream(newFilePath,
                    //    FileMode.Create, FileAccess.Write))
                    //{
                    //    fsNew.Write(bytes, 0, numBytesToRead);
                    //}
                }
            }

            channel.Close();
            connection.Close();
        }
        
    }
}