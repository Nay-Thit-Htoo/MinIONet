using Minio;
using MinIONet.Service.Models;

namespace MinIONet.Service.IServices
{
    public interface IValidatorService
    {
        MinIONetServiceResponse<string> ValidateMinIOClientArgs(MinIONetServiceRequest serviceReq);
        MinIONetServiceResponse<string> ValidateUploadRequest(UploadRequestArgs uploadReqArgs);
        MinIONetServiceResponse<string> ValidateDownloadRequest(DownloadRequestArgs downReqArgs);
        MinIONetServiceResponse<string> ValidateRemoveRequest(RemoveRequestArgs removeReqArgs);
        MinIONetServiceResponse<string> CheckMaxFileSize(long maxByteFileSize, UploadRequestArgs uploadReqArgs);
        MinIONetServiceResponse<string> CheckFileContentType(List<string> fileTypeLst, string fileContentType);
        Task<MinIONetServiceResponse<string>> CheckMinIOConnection(IMinioClient minioClient);
        Task<MinIONetServiceResponse<string>> CheckBucketExist(IMinioClient minioClient, string bucketName);
        Task<MinIONetServiceResponse<string>> CheckFileExist(IMinioClient minioClient, string bucketName, string fileName);

    }
}
