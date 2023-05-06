
namespace MessageQueues.Task1.MainProcessingService.Helpers
{
    public static class DirectoryHelper
    {
        public static string GetWorkingDirectory(string baseDirectory, string directory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new ArgumentNullException(nameof(baseDirectory));
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!Directory.Exists(baseDirectory))
            {
                throw new ArgumentException(nameof(baseDirectory));
            }

            var dataDirectory = $@"{directory}\";
            var currentDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\"));
            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);

            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }

            return workingDirectory;
        }
    }
}
