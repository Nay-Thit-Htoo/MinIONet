namespace MinIONet.Domain.Models
{
    public class UploadRequestArgs 
    {
        public string FileName { get; set; } = string.Empty;
        public string FromFilePath { get; set; } = string.Empty;
        public string ToFilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public bool IsRestructFilePath { get; set; }=false;
        public bool IsOverwriteFile { get; set; } = false;
    }
}
