namespace GameStore.Backend.Services;

public interface IFileStorageService
{
    Task<FileUploadResult> SaveAsync(IFormFile file);
}
