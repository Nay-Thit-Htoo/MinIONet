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

### Initialization
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
    MaxByteFileSize = 102400,//byte format
    AccessFileContentType = new List<string>() { "text/plain", "image/png", "image/jpeg", "application/vnd.ms-excel" }
};

MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
```
### Response Format 

```csharp
{
  MessageCode=StatusCode.Success,
  Messsage="Your Message",
  Result=,//<T> Generic type
}
```
### Status Codes

```csharp
 public enum StatusCode
 {
     Success = 00,
     Error = 01,
     NotFound = 02,
     RequireArgs=03,
     ExceptionOccur=04,
     ConnectionFail=05,
     FileAlreadyExist=06,
     ReachMaxFileSize=07,
     NotMatchFileContentType=08
 }
```
### Upload File

```csharp
MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
UploadRequestArgs uploadRequestArgs = new UploadRequestArgs()
{
    FileName = "your-file-name",
    FileType = "text/plain",
    FromFilePath = "local-file-choosen-path",
    IsRestructFilePath = true, //remove special character from file name
    IsOverwriteFile = true, //checking file already exist before upload
    ToFilePath = "/bucketname/current-date"
};
var uploadResult=await minIONetService.UploadFile(uploadRequestArgs); //will return uploaded file name

//Message Code
uploadResult.MessageCode

//Message
uploadResult.Message

//Result
uploadResult.Result
```

### Download File

```csharp
MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
DownloadRequestArgs downloadRequestArgs = new DownloadRequestArgs()
{
   // for no nested folder path
    FileName = "your-file-name.extension"

    //for nested folder path
    FileName="/bucketname/parent-file-name/your-file-name.extension"
};
var downloadResult=await minIONetService.DownloadFile(downloadRequestArgs);//will return file result with byte format

//Message Code
downloadResult.MessageCode

//Message
downloadResult.Message

//Result
downloadResult.Result
```

### Remove File

```csharp
MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
RemoveRequestArgs removeRequestArgs = new RemoveRequestArgs()
{
    // for no nested folder path
    //FileName = "your-file-name"

    //for nested folder path
    FileName = "/bucketname/parent-file-name/your-file-name"
};
var removeResult=await minIONetService.RemoveFile(removeRequestArgs);

//Message Code
removeResult.MessageCode

//Message
removeResult.Message

//Result
removeResult.Result
```


### Get Bucket Files

```csharp
MinIONetService minIONetService = new MinIONetService(minIONetServiceRequest);
var fileResult = await minIONetService.GetFiles("searchFileName"); //file name that you want to search 
foreach(var file in fileResult.Result)
  Console.WriteLine($"File Name : {file.Key}\n File Type: {file.ContentType}\n File Size: {file.Size}");
```

## Contact

Email - [naythithtoo000@gmail.com](mailto:naythithtoo000@gmail.com)
