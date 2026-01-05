using System;

namespace GameStore.Backend.Dtos;

public class UploadGameMediaDto
{
    public IFormFile File { get; set; } = null!;
}
