using System;

namespace GameStore.Backend.Dtos;

public class UploadFileDto
{
    public IFormFile? File { get; set; } = null;
}
