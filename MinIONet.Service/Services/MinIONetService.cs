using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MinIONet.Domain.Enums;
using MinIONet.Domain.IServices;
using MinIONet.Domain.Models;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace MinIONet.Service.Services
{
    public class MinIONetService : IMinIONetService
    {
        private MinIONetServiceRequest serviceRequest;
        private IMinioClient minioClient;
        private readonly IValidatorService validatorService; 
        public MinIONetService(MinIONetServiceRequest serviceReq)
        {
            validatorService = new ValidatorService();
            serviceRequest = serviceReq; 
        }       
        private async Task<(string MessageCode,string Message)> CheckMinIOClientArgs(Func<MinIONetServiceResponse<string>>? funReqValidation=null)
        {
            //Validate MinIO Client Args
            var response=validatorService.ValidateMinIOClientArgs(serviceRequest);

            //Initialized MinIO client
            if (response.MessageCode.Equals(nameof(StatusCode.Success))  && minioClient is null)                
                minioClient = new MinioClient()
                             .WithEndpoint(serviceRequest.EndPoint)
                             .WithCredentials(serviceRequest.AccessKey, serviceRequest.SecretKey)
                             .WithSSL(serviceRequest.IsAcceptHttps)
                             .Build();

            //Check MinIO Connection
            if (response.MessageCode.Equals(nameof(StatusCode.Success)))
                response = await validatorService.CheckMinIOConnection(minioClient);

            //Check Bucket Exist or Not
            if (response.MessageCode.Equals(nameof(StatusCode.Success)))
                response=await validatorService.CheckBucketExist(minioClient,serviceRequest.BucketName);

            //Validate Request
            if (response.MessageCode.Equals(nameof(StatusCode.Success)) && funReqValidation is not null)
                response = funReqValidation();
                
            return (response.MessageCode,response.Message);
        }
        public async Task<MinIONetServiceResponse<string>> UploadFile(UploadRequestArgs uploadReqArgs)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            var validate_result = await CheckMinIOClientArgs(() => validatorService.ValidateUploadRequest(uploadReqArgs));
            if (!validate_result.MessageCode.Equals(nameof(StatusCode.Success)))
            {
                response.MessageCode = validate_result.MessageCode;
                response.Message = validate_result.Message;
                goto Result;
            }

            try
            {                
                //remove special character
                RestructureFileName(uploadReqArgs);

                //check client specification
                response = await CheckClientSpecification(uploadReqArgs);
                if (!response.MessageCode.Equals(nameof(StatusCode.Success)))
                    goto Result;

                //Upload File to Bucket
                if (String.IsNullOrEmpty(uploadReqArgs.ToFilePath))
                    await minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(serviceRequest.BucketName)
                    .WithObject(uploadReqArgs.FileName)
                    .WithContentType(uploadReqArgs.FileType)
                    .WithFileName(uploadReqArgs.FromFilePath));//is reading from local file path
                else
                    await minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(serviceRequest.BucketName)
                    .WithObject($"{uploadReqArgs.ToFilePath}/{uploadReqArgs.FileName}")
                    .WithContentType(uploadReqArgs.FileType)
                    .WithFileName(uploadReqArgs.FromFilePath));
                response.Message = "File Successfully Uploaded";
            }
            catch (BucketNotFoundException ex)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = "Bucket Not Found !";             
            }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message =e.Message;                
            }

        Result:
            return response;
        }
        public async Task<MinIONetServiceResponse<byte[]>> DownloadFile(DownloadRequestArgs downloadReqArgs)
        {
            MinIONetServiceResponse<byte[]> response = new MinIONetServiceResponse<byte[]>();            
            var validate_result = await CheckMinIOClientArgs(() => validatorService.ValidateDownloadRequest(downloadReqArgs));
            if (!validate_result.MessageCode.Equals(nameof(StatusCode.Success)))
            {
                response.MessageCode = validate_result.MessageCode;
                response.Message = validate_result.Message;
                goto Result;
            }                

            try
            {
                await minioClient.GetObjectAsync(new GetObjectArgs()
                        .WithBucket(serviceRequest.BucketName)
                        .WithObject(downloadReqArgs.FileName)
                        .WithCallbackStream(async (stream) =>   // Provide a callback to handle the stream
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memoryStream); // Copy data to memory
                                response.Result=memoryStream.ToArray(); // Convert memory stream to byte array                               
                            }
                        }));
                response.Message = "File Successfully Downloaded!";                
            }
            catch (ObjectNotFoundException ex)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = "File Not Found !";
                response.Result = new byte[0];
            }
            catch (BucketNotFoundException ex)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message ="Bucket Not Found !";
                response.Result = new byte[0];
            }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = e.Message;
                response.Result=new byte[0];              
            }           
        Result:
            return response;
        }
        public async Task<MinIONetServiceResponse<string>> RemoveFile(RemoveRequestArgs removeReqArgs)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();
            var validate_result = await CheckMinIOClientArgs(() => validatorService.ValidateRemoveRequest(removeReqArgs));
            if (!validate_result.MessageCode.Equals(nameof(StatusCode.Success)))
            {
                response.MessageCode = validate_result.MessageCode;
                response.Message = validate_result.Message;
                goto Result;
            }

            try
            {
                //Remove File 
                var args = new RemoveObjectArgs()
                    .WithBucket(serviceRequest.BucketName)
                    .WithObject(removeReqArgs.FileName);
                await minioClient.RemoveObjectAsync(args);
                response.Message = "File Successfully Removed!";
            }
            catch (BucketNotFoundException ex)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = "Bucket Not Found !";               
            }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = e.Message;
            }

        Result:
            return response;

        }
        public async Task<MinIONetServiceResponse<IEnumerable<Item>>> GetFiles(string searchFileName)
        {            
            MinIONetServiceResponse<IEnumerable<Item>> response = new MinIONetServiceResponse<IEnumerable<Item>>();
            var validate_result = await CheckMinIOClientArgs();
            if (!validate_result.MessageCode.Equals(nameof(StatusCode.Success)))
            {
                response.MessageCode = validate_result.MessageCode;
                response.Message=validate_result.Message;
                goto Result;
            }

            try
            {
                var listArgs = (String.IsNullOrEmpty(searchFileName)) ?
                    new ListObjectsArgs()
                .WithBucket(serviceRequest.BucketName)
                .WithRecursive(true) :
                 new ListObjectsArgs()
                .WithBucket(serviceRequest.BucketName)
                .WithPrefix(searchFileName)
                .WithRecursive(true);

                var bucketObjectList = minioClient.ListObjectsEnumAsync(listArgs);
                if (bucketObjectList is null)
                {
                    response.MessageCode = nameof(StatusCode.NotFound);
                    response.Message = $"There is no any objects for {serviceRequest.BucketName}! 👾";                   
                }

                List<Item> fileItemLst=new List<Item>();
                await foreach (Item item in bucketObjectList)
                {
                   fileItemLst.Add(item);
                }
                response.Result = fileItemLst;
            }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = e.Message;                
            }

        Result:
            return response;
        }
        public async Task<MinIONetServiceResponse<byte[]>> DownloadAllFiles()
        {
            MinIONetServiceResponse<byte[]> response = new MinIONetServiceResponse<byte[]>();
            var validate_result = await CheckMinIOClientArgs();
            if (!validate_result.MessageCode.Equals(nameof(StatusCode.Success)))
            {
                response.MessageCode = validate_result.MessageCode;
                response.Message = validate_result.Message;
                goto Result;
            }

            try
            {
                using (var zipMemoryStream = new MemoryStream())
                {
                    // Create a ZIP archive in the memory stream
                    using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
                    {
                        // List all files (objects) in the bucket
                        var listArgs = new ListObjectsArgs()
                                        .WithBucket(serviceRequest.BucketName)
                                        .WithRecursive(true);
                        var objectList= minioClient.ListObjectsEnumAsync(listArgs);
                        await foreach (Item obj in objectList)
                        {
                            string fileName = obj.Key;

                            // Create a stream to download the object
                            using (var objectStream = new MemoryStream())
                            {
                                // Download the object from MinIO into the memory stream
                                await minioClient.GetObjectAsync(new GetObjectArgs()
                                  .WithBucket(serviceRequest.BucketName)
                                  .WithObject(fileName)
                                  .WithCallbackStream(async (stream) =>   // Provide a callback to handle the stream
                                  {
                                      await stream.CopyToAsync(objectStream); // Copy data to memory
                                  }));                         

                                // Reset the position of the stream to read from the beginning
                                objectStream.Position = 0;

                                // Add the file to the ZIP archive
                                var zipEntry = archive.CreateEntry(fileName);
                                using (var entryStream = zipEntry.Open())
                                {
                                    objectStream.CopyTo(entryStream);
                                }
                            }
                        }
                    }

                    // Reset the memory stream position to the beginning to return it as a byte array
                    zipMemoryStream.Position = 0;                    

                    // Convert the ZIP file in memory to a byte array
                    response.Result= zipMemoryStream.ToArray();

                }
                response.Message = "Files Successfully Downloaded!";
            }
            catch (BucketNotFoundException ex)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = "Bucket Not Found !";
                response.Result = new byte[0];
            }
            catch (MinioException e)
            {
                response.MessageCode = nameof(StatusCode.Error);
                response.Message = e.Message;
                response.Result = new byte[0];
            }
        Result:
            return response;
        }
        private void RestructureFileName(UploadRequestArgs uploadReqArgs)
        {
            if(uploadReqArgs.IsRestructFilePath)
            {
                var fileNameLst= uploadReqArgs.FileName.Split('.');
                string repSpaceUndersocre = fileNameLst.First().Replace(" ", "_");
                uploadReqArgs.FileName = $"{Regex.Replace(repSpaceUndersocre, @"[^a-zA-Z0-9_]", "")}.{fileNameLst.Last()}";
            }           
        }
        private async Task<MinIONetServiceResponse<string>> CheckClientSpecification(UploadRequestArgs uploadReqArgs)
        {
            MinIONetServiceResponse<string> response = new MinIONetServiceResponse<string>();

            //check file content type       
            if (serviceRequest.AccessFileContentType is not null && serviceRequest.AccessFileContentType.Count()>0)
                response = validatorService.CheckFileContentType(serviceRequest.AccessFileContentType, uploadReqArgs.FileType!);
                if (!response.MessageCode.Equals(nameof(StatusCode.Success)))
                   goto Result;

            //check file size
            if (serviceRequest.MaxByteFileSize > 0)
                response = validatorService.CheckMaxFileSize(serviceRequest.MaxByteFileSize, uploadReqArgs);
                if (!response.MessageCode.Equals(nameof(StatusCode.Success)))
                    goto Result;

            //check allow overwirte
            if (!uploadReqArgs.IsOverwriteFile)
            {
                response=await validatorService.CheckFileExist(minioClient,serviceRequest.BucketName,uploadReqArgs.FileName);
                if (!response.MessageCode.Equals(nameof(StatusCode.Success)))
                    goto Result;
            }           

        Result:
            return response;

        }
    }
}
