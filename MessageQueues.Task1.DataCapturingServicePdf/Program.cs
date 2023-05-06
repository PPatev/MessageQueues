using RabbitMQ.Client;

namespace MessageQueues.Task1.DataCapturingServicePdf
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataCaptureApp.Start();
        }
    }
}