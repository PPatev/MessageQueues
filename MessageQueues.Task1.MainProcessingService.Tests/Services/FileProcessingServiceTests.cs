using MessageQueues.Task1.MainProcessingService.Helpers;
using MessageQueues.Task1.MainProcessingService.Interfaces;
using MessageQueues.Task1.MainProcessingService.Models;
using MessageQueues.Task1.MainProcessingService.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace MessageQueues.Task1.MainProcessingService.Tests.Services
{
    [TestFixture]
    public class FileProcessingServiceTests
    {
        private Mock<IOptions<FileProcessingConfig>> _fileProcessingOptions;
        private Mock<IMessageConsumer> _messageConsumer;
        private FileProcessingService _fileProcessingService;

        [SetUp]
        public void Setup()
        {
            
            _fileProcessingOptions = new Mock<IOptions<FileProcessingConfig>>();
            _fileProcessingOptions.SetupGet(x => x.Value).Returns(new FileProcessingConfig { OutputDirectory = "TestFolder" });
            _messageConsumer = new Mock<IMessageConsumer>();

            _fileProcessingService = new FileProcessingService(_fileProcessingOptions.Object, _messageConsumer.Object);
        }

        [Test]
        public void StartProcessing_ShouldCallMessageConsumer()
        {
            var expectedDirectory = DirectoryHelper.GetWorkingDirectory(AppContext.BaseDirectory, _fileProcessingOptions.Object.Value.OutputDirectory);
            var input = new StringReader("d");
            Console.SetIn(input);

            _fileProcessingService.StartProcessing();

            _messageConsumer.Verify(x => x.StartConsuming(expectedDirectory), Times.Once);
        }
    }
}
