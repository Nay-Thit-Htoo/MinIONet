<img src="https://github.com/Nay-Thit-Htoo/MinIONet/blob/main/landingpage.png"/>

# MinIO.NET

MinIO with a .NET application allows you to interact with MinIO's object storage via its S3-compatible API. MinIO is a high-performance, distributed object storage system that is often used for building private cloud storage. Here's a basic guide on integrating MinIO with .NET using the MinIO SDK for .NET.

## Table of Contents

- Installation
- Usage

### Installation

Provide the steps to install the package, either through NuGet or via direct download.
```bash
  dotnet add package MinIO.NET --version 1.1.1
```
or

You can also install via Nuget Package Manager by Searching `MinIO.NET`

## Usage

### Basic Example
```csharp
using MinIONet.Service.Models;
using MinIONet.Service.Services;

MinIONetServiceRequest minIONetServiceRequest = new MinIONetServiceRequest()
{
    EndPoint = "your-minio-api-endpoint",
    AccessKey = "your-minio-access-key",
    SecretKey = "your-minio-secret-key",
    BucketName = "your-bucket-name",
    IsAcceptHttps = false,//true for https
    MaxByteFileSize = 102400,//with byte
    AccessFileContentType = new List<string>() { "text/plain", "image/png", "image/jpeg", "application/vnd.ms-excel" }
};

MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
```

## Contact

Email - [naythithtoo000@gmail.com](mailto:naythithtoo000@gmail.com)
