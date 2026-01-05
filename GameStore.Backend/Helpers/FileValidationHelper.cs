namespace GameStore.Backend.Helpers;

public class FileValidationHelper
{
    public static readonly string[] AllowedExtensions =
        [
        ".jpg", ".jpeg", ".png", ".pdf"
    ];
    public static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "application/pdf"
    ];

    private const long MaxImageSize = 2 * 1024 * 1024;
    private const long MaxPdfSize = 5 * 1024 * 1024;

    public static (bool IsValid, string? Error) Validate(IFormFile file)
    {
        //File Empty Checks
        if (file.Length == 0 || file == null)
            return (false, "File is empty");

        //Extension Check
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (false, "Invalid file type");

        //Type check
        if (!AllowedContentTypes.Contains(file.ContentType))
            return (false, "Invalid file content type");

        //Size check
        if (extension == ".pdf" && file.Length > MaxPdfSize)
            return (false, "PDF file size exceeds 5 MB");
        if ((extension == ".jpg" || extension == ".jpeg" || extension == ".png") && file.Length > MaxImageSize)
            return (false, "Image file size exceeds 2 MB");

        return (true, null);
    }
}
