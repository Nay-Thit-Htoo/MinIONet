using Minio.Exceptions;
using Minio;
using MinIONet.Domain.Enums;
using MinIONet.Domain.IServices;
using MinIONet.Domain.Models;
using Minio.DataModel.Args;

namespace MinIONet.Service.Services
{
    public class ValidatorService : IValidatorService
    {
        public MinIONetServiceResponse<string> ValidateMinIOClientArgs(MinIONetServiceRequest serviceReq)
        {
            MinIONetServiceResponse<string> result=new MinIONetServiceResponse<string>();
            result.MessageCode = nameof(StatusCode.RequireArgs);
            if (string.IsNullOrEmpty(serviceReq.EndPoint))
            {
                result.Message = "Endpoint is required!";
                goto Result;
            }

            if (string.IsNullOrEmpty(serviceReq.AccessKey))
            {
                result.Message = "AccessKey is required!";
                goto Result;
            }

            if (string.IsNullOrEmpty(serviceReq.SecretKey))
            {
                result.Message = "SecretKey is required!";
                goto Result;
            }

            if (string.IsNullOrEmpty(serviceReq.BucketName))
            {
                result.Message = "BucketName is required!";
                goto Result;
            }            

            result.MessageCode = nameof(StatusCode.Success);
            Result:
              return result;
        }
        public MinIONetServiceResponse<string> ValidateUploadRequest(UploadRequestArgs uploadReqArgs)
        {
            MinIONetServiceResponse<string> result = new MinIONetServiceResponse<string>();
            result.MessageCode = nameof(StatusCode.RequireArgs);
            if (string.IsNullOrEmpty(uploadReqArgs.FileName))
            {
                result.Message = "File Name is required!";
                goto Result;
            }

            //if (string.IsNullOrEmpty(uploadReqArgs.ToFilePath))
            //{
            //    result.Message = "To File Path is required!";
            //    goto Result;
            //}
           
            result.MessageCode = nameof(StatusCode.Success);
          
        Result:
            return result;                    
        }
        public MinIONetServiceResponse<string> ValidateDownloadRequest(DownloadRequestArgs downReqArgs)
        {
            MinIONetServiceResponse<string> result = new MinIONetServiceResponse<string>();
            result.MessageCode = nameof(StatusCode.RequireArgs);
            if (string.IsNullOrEmpty(downReqArgs.FileName))
            {
                result.Message = "File Name is required!";
                goto Result;
            }   
            result.MessageCode = nameof(StatusCode.Success);

        Result:
            return result;
        }
        public MinIONetServiceResponse<string> ValidateRemoveRequest(RemoveRequestArgs removeReqArgs)
        {
            MinIONetServiceResponse<string> result = new MinIONetServiceResponse<string>();
            result.MessageCode = nameof(StatusCode.RequireArgs);
            if (string.IsNullOrEmpty(removeReqArgs.FileName))
            {
                result.Message = "File Name is required!";
                goto Result;
            }
            result.MessageCode = nameof(StatusCode.Success);

        Result:
            return result;
        }
        public MinIONetServiceResponse<string> CheckMaxFileSize(long maxByteFileSize,UploadRequestArgs uploadReqArgs)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            try
            {
                // Create a FileInfo object for the selected file
                FileInfo fileInfo = new FileInfo(uploadReqArgs.FromFilePath);

                // Check if the file exists
                if (fileInfo.Exists)
                {                    

                    // Get the file size in bytes
                    long fileSizeInBytes = fileInfo.Length;
                    if (fileSizeInBytes > maxByteFileSize)
                    {
                        response.Message = $"Maximum allowed file size is {maxByteFileSize} Bytes";
                        response.MessageCode = nameof(StatusCode.ReachMaxFileSize);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.MessageCode = nameof(StatusCode.ExceptionOccur);
            }        
            return response;

        }
        public async Task<MinIONetServiceResponse<string>> CheckMinIOConnection(IMinioClient minioClient)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            try
            {
                var bucketList = await minioClient.ListBucketsAsync();
                if (bucketList is null || bucketList.Buckets is null)
                {
                    response.Message = "MinIO Connection Failed!";
                    response.MessageCode = nameof(StatusCode.ConnectionFail);
                }
            }

            catch (MinioException e)
            {
                response.Message = $"MinIO connection failed : {e.Message}";
                response.MessageCode = nameof(StatusCode.ExceptionOccur);
            }

            return response;
        }
        public async Task<MinIONetServiceResponse<string>> CheckBucketExist(IMinioClient minioClient,string bucketName)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            try
            {
                bool bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
                if (!bucketExists)
                {
                    response.Message = "Bucket Not Found !";
                    response.MessageCode = nameof(StatusCode.NotFound);
                }
            }
            catch (MinioException e)
            {
                response.Message = $"[Exception] [Check Bucket Exist] : {e.Message}";
                response.MessageCode = nameof(StatusCode.ExceptionOccur);
            }

            return response;
        }
        public async Task<MinIONetServiceResponse<string>>CheckFileExist(IMinioClient minioClient,string bucketName,string fileName)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            try
            {
                //Check File Exist
                var args = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);
                var objectStat = await minioClient.StatObjectAsync(args);
                response.MessageCode = nameof(StatusCode.FileAlreadyExist);
                response.Message = $"{fileName} Already Exist !";
            }
            catch (Minio.Exceptions.ObjectNotFoundException)
            { goto Result; }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.ExceptionOccur);
                response.Message = e.Message;
            }

        Result:
            return response;

        }
        public MinIONetServiceResponse<string> CheckFileContentType(List<string>fileTypeLst,string fileContentType)
        {
            MinIONetServiceResponse<string> result = new MinIONetServiceResponse<string>();
            if(!fileTypeLst.Any((x)=>x.Contains(fileContentType)))
            {
                result.MessageCode = nameof(StatusCode.NotMatchFileContentType);
                result.Message = $"Not allow file content type {fileContentType}!";                          
            }         
            return result;
        }
    }
}
