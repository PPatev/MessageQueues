
namespace MessageQueues.Task1.DataCapturingServicePdf.Intefaces
{
    public interface IMessageProducer
    {
        void SendBytes(string fileName, string fileType, IList<byte[]> fileBytes);
    }
}
