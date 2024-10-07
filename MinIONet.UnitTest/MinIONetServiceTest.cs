using Minio;
using MinIONet.Domain.Enums;
using MinIONet.Domain.Models;
using MinIONet.Service.Services;
using Moq;


namespace MinIONet.UnitTest
{
    public class MinIOServiceTest
    {

        private Mock<MinioClient> _mockMinioClient;
        private MinIONetService _minioService;
        private MinIONetServiceRequest requestDto;
        [SetUp]
        public void Setup()
        {           
            requestDto= new MinIONetServiceRequest()
            {
                EndPoint = "localhost:900",
                AccessKey = "minioadmin",
                SecretKey= "minioadmin"
            };
            _minioService = new MinIONetService(requestDto);
            _mockMinioClient = new Mock<MinioClient>();
        }     


        [Test]        
        //[MethodName]_[Scenario]_[ExpectedResult]
        public async Task UploadFile_SuccessfullyUpload_ReturnSuccess()
        {
            //[Arrange]
            UploadRequestArgs fileRequestDto = new UploadRequestArgs()
            {                
                FileName = "test-object",
                FromFilePath = "C:/Users/naythithtoo.ABANK/Downloads/test.txt"
            };            

            //[Action]           
            var result =await _minioService.UploadFile(fileRequestDto);

            //[Assert]
            Assert.IsNotNull(result);
            Assert.AreEqual(result.MessageCode,nameof(StatusCode.Success));            
        }
    }
}