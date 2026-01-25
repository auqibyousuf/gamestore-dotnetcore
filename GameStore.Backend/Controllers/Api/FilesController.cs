using GameStore.Backend.Dtos;
using GameStore.Backend.Helpers;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController(IFileStorageService fileStorageService) : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto dto)
        {
            if (dto.File == null)
                return BadRequest("No file uploaded");

            var result = await _fileStorageService.SaveAsync(dto.File);
            return Ok(result);
        }
    }
}
