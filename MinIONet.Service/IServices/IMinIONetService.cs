﻿using Minio.DataModel;
using MinIONet.Domain.Models;

namespace MinIONet.Service
{
    public interface IMinIONetService
    {
        Task<MinIONetServiceResponse<IEnumerable<Item>>> GetFiles(string searchFileName);
        Task<MinIONetServiceResponse<string>> UploadFile(UploadRequestArgs uploadReqArgs);
        Task<MinIONetServiceResponse<byte[]>> DownloadFile(DownloadRequestArgs downloadReqArgs);
        Task<MinIONetServiceResponse<string>> RemoveFile(RemoveRequestArgs removeReqArgs);
        Task<MinIONetServiceResponse<byte[]>> DownloadAllFiles();
    }
}
