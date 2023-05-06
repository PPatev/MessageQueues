
namespace MessageQueues.Task1.MainProcessingService.Interfaces
{
    public interface IMessageConsumer
    {
        void StartConsuming(string outputDirectory);
    }
}
