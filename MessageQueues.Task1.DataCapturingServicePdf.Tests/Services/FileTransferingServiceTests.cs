
using AutoFixture;
using FluentAssertions;
using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using MessageQueues.Task1.DataCapturingServicePdf.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace MessageQueues.Task1.DataCapturingServicePdf.Tests.Services
{
    [TestFixture]
    public class FileTransferingServiceTests
    {
        private const int LoremIpsumFileSize = 100354;
        private const string LoremIpsumFileName = "Lorem Ipsum.txt";

        private Mock<IOptions<FileTransferConfig>> _fileTransferOptions;
        private Mock<IMessageProducer> _messageProducer;
        private ITransferingService _transferService;


        [SetUp]
        public void Setup()
        {
            _fileTransferOptions = new Mock<IOptions<FileTransferConfig>>();
            _fileTransferOptions.SetupGet(x => x.Value)
                .Returns(new FileTransferConfig
                {
                    DataDirectory = "TestFolder",
                    FileType = "txt",
                    ChunkSize = 10240,
                });

            _messageProducer = new Mock<IMessageProducer>();

            _transferService = new FileTransferingService(_fileTransferOptions.Object, _messageProducer.Object);
        }

        [Test]
        public void StartTransfering_WithNotEmptyDirectory_ShouldCallMessageProducer()
        {
            var fileType = _fileTransferOptions.Object.Value.FileType;
            var countDouble = LoremIpsumFileSize / (double)_fileTransferOptions.Object.Value.ChunkSize;
            var count = (int)Math.Ceiling(countDouble);

            _transferService.StartTransfering();

            _messageProducer.Verify(x => x.SendBytes(LoremIpsumFileName, fileType, It.Is<IList<byte[]>>(x => x.Count == count)), Times.Once);
        }

        [Test]
        public void StartTransfering_WithDifferentFileType_ShouldNotCallMessageProducer()
        {
            var fileTransferOptions = new Mock<IOptions<FileTransferConfig>>();
            fileTransferOptions.SetupGet(x => x.Value)
                .Returns(new FileTransferConfig
                {
                    DataDirectory = "TestFolder",
                    FileType = "pdf",
                    ChunkSize = 10240,
                });

            var transferService = new FileTransferingService(fileTransferOptions.Object, _messageProducer.Object);

            transferService.StartTransfering();

            _messageProducer.Verify(x => x.SendBytes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<byte[]>>()), Times.Never);
        }

        [Test]
        public void StartTransfering_WithEmptyDirectory_ShouldNotCallMessageProducer()
        {
            var fileTransferOptions = new Mock<IOptions<FileTransferConfig>>();
            fileTransferOptions.SetupGet(x => x.Value)
                .Returns(new FileTransferConfig
                {
                    DataDirectory = "EmptyTestFolder",
                    FileType = "txt",
                    ChunkSize = 10240,
                });

            var transferService = new FileTransferingService(fileTransferOptions.Object, _messageProducer.Object);

            transferService.StartTransfering();

            _messageProducer.Verify(x => x.SendBytes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<byte[]>>()), Times.Never);
        }

        [Test]
        public void StartTransfering_WithNonExistingDirectory_ShouldThrowArgumentException()
        {
            var fileTransferOptions = new Mock<IOptions<FileTransferConfig>>();
            fileTransferOptions.SetupGet(x => x.Value)
                .Returns(new FileTransferConfig
                {
                    DataDirectory = "NonExistingFolder",
                    FileType = "txt",
                    ChunkSize = 10240,
                });

            var transferService = new FileTransferingService(fileTransferOptions.Object, _messageProducer.Object);

            var act = (() => transferService.StartTransfering());

            act.Should().Throw<ArgumentException>();
            _messageProducer.Verify(x => x.SendBytes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<byte[]>>()), Times.Never);
        }
    }
}
