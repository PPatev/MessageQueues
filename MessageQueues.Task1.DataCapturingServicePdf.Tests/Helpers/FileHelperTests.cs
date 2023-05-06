using AutoFixture;
using FluentAssertions;
using MessageQueues.Task1.DataCapturingServicePdf.Helpers;
using NUnit.Framework;

namespace MessageQueues.Task1.DataCapturingServicePdf.Tests.Helpers
{
    [TestFixture]
    public class FileHelperTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp() { _fixture = new Fixture(); }

        [Test]
        [TestCase(null, null, null)]
        [TestCase("someValue", null, null)]
        [TestCase("someValue", "someValue", null)]
        public void GetFilesFromDirectory_OnIcorrectArguments_ShouldThrowArgumentNullException(string baseDirectory, string directory, string fileType)
        { 
            var act = () => FileHelper.GetFilesFromDirectory(baseDirectory, directory, fileType);

            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetFilesFromDirectory_OnNonExistingDirectory_ShouldThrowArgumentException()
        {
            var baseDirectory = _fixture.Create<string>();
            var directory = _fixture.Create<string>();

            var act = () => FileHelper.GetFilesFromDirectory(baseDirectory, directory, "txt");

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("TestFolder", 1)]
        [TestCase("EmptyTestFolder", 0)]
        public void GetFilesFromDirectory_OnExistingDirectory_ShouldReturnCorrect(string existingDirectory, int filesCount)
        {
            var baseDirectory = AppContext.BaseDirectory;

            var result = FileHelper.GetFilesFromDirectory(baseDirectory, existingDirectory, "txt");

            result.Length.Should().Be(filesCount);
        }
    }
}
