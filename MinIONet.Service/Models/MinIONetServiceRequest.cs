namespace MinIONet.Domain.Models
{
    public class MinIONetServiceRequest
    {
        public string EndPoint { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool IsAcceptHttps { get; set; } = false;
        public long MaxByteFileSize { get; set; } = 125000;//1MB
        public List<string>? AccessFileContentType { get; set; }
    }
}
