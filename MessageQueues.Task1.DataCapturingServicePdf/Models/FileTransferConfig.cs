namespace MessageQueues.Task1.DataCapturingServicePdf.Models
{
    public class FileTransferConfig
    {
        
        public string DataDirectory { get; set; }

        public string FileType { get; set; }

        public int ChunkSize { get; set; }
    }
}
