using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinIONet.Domain.Enums;
using MinIONet.Domain.Models;
using MinIONet.Service;

namespace MinIONet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinIONetController : ControllerBase
    {
        private readonly IMinIONetService minIONetService;
        public MinIONetController(IMinIONetService _minIONetService)
        {
            minIONetService = _minIONetService;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(string? ToFilePath, IFormFile File)
        {
            if (File == null || File.Length == 0)
                return BadRequest("File is missing");

            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await File.CopyToAsync(stream);
            }          
            var result=await minIONetService.UploadFile(new UploadRequestArgs() { FileName= File.FileName,FromFilePath=filePath,ToFilePath= ToFilePath!,FileType= File.ContentType,IsRestructFilePath=true,IsOverwriteFile=false});
            return Ok(result);
        }

        [HttpPost("DownloadFile")]
        public async Task<IActionResult> DownloadFile(string FileName)
        {
            var request = new DownloadRequestArgs() { FileName = FileName};            
            var result = await minIONetService.DownloadFile(request);
            if(!result.MessageCode.Equals(nameof(Domain.Enums.StatusCode.Success))) 
                 return Ok(result);
            return File(result.Result!, "application/octet-stream", FileName);
        }

        [HttpPost("DownloadAllFiles")]
        public async Task<IActionResult> DownloadAllFiles()
        {            
            var result = await minIONetService.DownloadAllFiles();
            return File(result.Result!, "application/zip", "bucket-files.zip");
        }

        [HttpPost("GetFiles")]
        public async Task<IActionResult> GetFiles(string? SearchFileName)
        {            
            return Ok(await minIONetService.GetFiles(SearchFileName!));
        }

        [HttpPost("RemoveFile")]
        public async Task<IActionResult> RemoveFile(string FileName)
        {
            var request = new RemoveRequestArgs() { FileName = FileName };
            return Ok(await minIONetService.RemoveFile(request));
        }
    }
}
