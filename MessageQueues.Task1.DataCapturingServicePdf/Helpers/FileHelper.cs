
namespace MessageQueues.Task1.DataCapturingServicePdf.Helpers
{
    public static class FileHelper
    {
        public static string[] GetFilesFromDirectory(string baseDirectory, string directory, string fileType)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory)) 
            { 
                throw new ArgumentNullException(nameof(baseDirectory));
            }

            if (string.IsNullOrWhiteSpace(directory)) 
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (string.IsNullOrWhiteSpace(fileType)) 
            {
                throw new ArgumentNullException(nameof(fileType));
            }

            var dataDirectory = @$"{directory}\";
            var currentDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\"));
            var workingDirectory = Path.Combine(currentDirectory, dataDirectory);
            var fileExtensions = $"*.{fileType}";

            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException(nameof(directory), "The provided directory doesn`t exist.");
            }

            var files = Directory.GetFiles(workingDirectory, fileExtensions);

            return files;
        }
    }
}
