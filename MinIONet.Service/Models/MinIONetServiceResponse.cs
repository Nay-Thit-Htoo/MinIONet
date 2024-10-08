using MinIONet.Domain.Enums;

namespace MinIONet.Domain.Models
{
    public class MinIONetServiceResponse<T>
    {
        public string MessageCode { get; set; } = nameof(StatusCode.Success);
        public string Message { get; set; } = "Success";
        public T? Result { get; set; }
    }
}
