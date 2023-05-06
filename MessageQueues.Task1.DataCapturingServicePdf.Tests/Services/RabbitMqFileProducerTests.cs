using AutoFixture;
using FluentAssertions;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using MessageQueues.Task1.DataCapturingServicePdf.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;

namespace MessageQueues.Task1.DataCapturingServicePdf.Tests.Services
{
    [TestFixture]
    public class RabbitMqFileProducerTests
    {
        private Fixture _fixture;
        private Mock<IConnection> _connection;
        private Mock<IModel> _channel;
        private Mock<IBasicProperties> _basicProperties;
        private Mock<IOptions<RabbitMqConfig>> _rabbitMqOptions;
        private RabbitMqFileProducer _rabbitMqFileProducer;

        [SetUp]
        public void Setup() 
        {
            _fixture = new Fixture();

            _connection = new Mock<IConnection>();

            _channel = new Mock<IModel>();
            _channel.SetupGet(x => x.IsOpen).Returns(true);

            _basicProperties = new Mock<IBasicProperties>();
            _basicProperties.SetupGet(x => x.Headers)
                .Returns(new Dictionary<string, object>
                {
                    { "output-file", "" },
                    { "finished", false }
                });

            _channel.Setup(x => x.CreateBasicProperties()).Returns(_basicProperties.Object);

            _connection.SetupGet(x => x.IsOpen).Returns(true);
            _connection.Setup(x => x.CreateModel()).Returns(_channel.Object);

            _rabbitMqOptions = new Mock<IOptions<RabbitMqConfig>>();
            _rabbitMqOptions.SetupGet(x => x.Value).Returns(_fixture.Create<RabbitMqConfig>());

            _rabbitMqFileProducer = new RabbitMqFileProducer(_connection.Object, _rabbitMqOptions.Object);
        }

        [Test]
        public void Dispose_OnIsOpenTrue_ShouldCallClose()
        { 
            _rabbitMqFileProducer.Dispose();

            _connection.Verify(x => x.Close(), Times.Once);
            _channel.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void Dispose_OnIsOpenFalse_ShouldNotCallClose()
        {
            var connection = new Mock<IConnection>();
            var channel = new Mock<IModel>();
            channel.SetupGet(x => x.IsOpen).Returns(false);

            connection.SetupGet(x => x.IsOpen).Returns(false);
            connection.Setup(x => x.CreateModel()).Returns(_channel.Object);

            var rabbitMqFileProducer = new RabbitMqFileProducer(connection.Object, _rabbitMqOptions.Object);

            rabbitMqFileProducer.Dispose();

            connection.Verify(x => x.Close(), Times.Never);
            channel.Verify(x => x.Close(), Times.Never);
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase("someValue", null, null)]
        [TestCase("someValue", "someValue", null)]
        public void SendBytes_OnIncorrectArguments_ShouldThrowArgumentException(string fileName, string fileType, IList<byte[]> fileBytes)
        { 
            var act = () => _rabbitMqFileProducer.SendBytes(fileName, fileType, fileBytes);

            act.Should().Throw<ArgumentException>();
            _channel.Verify(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()), Times.Never);
        }

        [Test]
        public void SendBytes_OnEmptyFileBytes_ShouldNotCallBasicPublish()
        {
            _rabbitMqFileProducer.SendBytes(_fixture.Create<string>(), _fixture.Create<string>(), new List<byte[]>());
            
            _channel.Verify(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()), Times.Never);
        }

        [Test]
        public void SendBytes_OnCorrectArguments_ShouldCallBasicPublish()
        {
            var expectedExchange = _rabbitMqOptions.Object.Value.ExchangeName;
            var expectedRoutineKey = _rabbitMqOptions.Object.Value.RoutingKey;
            var bytesList = _fixture.CreateMany<byte[]>().ToList();

            _rabbitMqFileProducer.SendBytes(_fixture.Create<string>(), _fixture.Create<string>(), bytesList);

            _channel.Verify(x => x.BasicPublish(expectedExchange, expectedRoutineKey, false, _basicProperties.Object, It.IsAny<ReadOnlyMemory<byte>>()), Times.Exactly(bytesList.Count));
        }
    }
}
