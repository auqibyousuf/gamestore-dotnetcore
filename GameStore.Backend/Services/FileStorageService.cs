using GameStore.Backend.Helpers;

namespace GameStore.Backend.Services;

public class FileStorageService(
    IWebHostEnvironment environment,
    IHttpContextAccessor httpContextAccessor) : IFileStorageService
{
    private readonly IWebHostEnvironment _environment = environment;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<FileUploadResult> SaveAsync(IFormFile file)
    {
        // 1️⃣ Validate file
        var (isValid, error) = FileValidationHelper.Validate(file);
        if (!isValid)
            throw new Exception(error);

        // 2️⃣ Generate GUID filename
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";

        // 3️⃣ Create uploads folder
        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        // 4️⃣ Save file
        var filePath = Path.Combine(uploadFolder, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // 5️⃣ Build public URL
        var request = _httpContextAccessor.HttpContext!.Request;
        var url = $"{request.Scheme}://{request.Host}/uploads/{fileName}";

        // 6️⃣ Return result
        return new FileUploadResult
        {
            FileName = fileName,
            Url = url
        };
    }
}
