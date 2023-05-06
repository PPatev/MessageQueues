
using AutoFixture;
using FluentAssertions;
using MessageQueues.Task1.MainProcessingService.Helpers;
using NUnit.Framework;
using System.IO;

namespace MessageQueues.Task1.MainProcessingService.Tests.Helpers
{
    [TestFixture]
    public class DirectoryHelperTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp() { _fixture = new Fixture(); }

        [Test]
        [TestCase(null, "someValue")]
        [TestCase("someValue", null)]
        public void GetWorkingDirectory_OnIcorrectArguments_ShouldThrowArgumentNullException(string baseDirectory, string directory)
        {
            var act = () => DirectoryHelper.GetWorkingDirectory(baseDirectory, directory);

            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetWorkingDirectory_OnNonExistingBaseDirectory_ShouldThrowArgumentException()
        {
            var baseDirectory = _fixture.Create<string>();
            var directory = _fixture.Create<string>();

            var act = () => DirectoryHelper.GetWorkingDirectory(baseDirectory, directory);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void GetWorkingDirectory_OnNonExistingWorkingDirectory_ShouldNotThrowExceptionAndCreate()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var newDirectorySuffix = _fixture.Create<int>();
            var directory = $"TestFolder{newDirectorySuffix}";

            var act = () => DirectoryHelper.GetWorkingDirectory(baseDirectory, directory);

            act.Should().NotThrow();
            Directory.Exists(act.Invoke()).Should().BeTrue();
        }

        [Test]
        [TestCase("TestFolder")]
        public void GetWorkingDirectory_OnExistingWorkingDirectory_ShouldReturnCorrect(string existingDirectory)
        {
            var baseDirectory = AppContext.BaseDirectory;

            var dataDirectory = $@"{existingDirectory}\";
            var currentDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\"));
            var expectedDirectory = Path.Combine(currentDirectory, dataDirectory);

            var result = DirectoryHelper.GetWorkingDirectory(baseDirectory, existingDirectory);

            result.Should().Be(expectedDirectory);
        }
    }
}
