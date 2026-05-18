namespace TechMove.GLMS.Core.Services;

public class FileService : IFileService
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    public const string AllowedExtension = ".pdf";
    public static readonly string[] AllowedContentTypes = { "application/pdf" };
    public const string StorageFolderRelative = "uploads/contracts";

    private readonly string _webRootPath;

    public FileService(string webRootPath)
    {
        if (string.IsNullOrWhiteSpace(webRootPath))
            throw new ArgumentException("Web root path is required.", nameof(webRootPath));
        _webRootPath = webRootPath;
    }

    public ValidationResult ValidateUpload(string? originalFileName, string? contentType, long sizeInBytes)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            errors.Add("No file was provided.");
            return ValidationResult.Failure(errors);
        }

        // Extension check — case-insensitive, only allow .pdf
        var extension = Path.GetExtension(originalFileName);
        if (!string.Equals(extension, AllowedExtension, StringComparison.OrdinalIgnoreCase))
            errors.Add($"Only PDF files are allowed. Provided extension: '{extension}'.");

        // Content-type check
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Invalid content type '{contentType}'. Expected application/pdf.");
        }

        // Size check
        if (sizeInBytes <= 0)
            errors.Add("File is empty.");
        else if (sizeInBytes > MaxFileSizeBytes)
            errors.Add($"File exceeds the maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    public async Task<(string StoragePath, string OriginalName)> SaveAsync(
        Stream uploadStream,
        string originalFileName,
        CancellationToken ct = default)
    {
        if (uploadStream is null) throw new ArgumentNullException(nameof(uploadStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original filename is required.", nameof(originalFileName));

        var absoluteFolder = Path.Combine(_webRootPath, StorageFolderRelative);
        Directory.CreateDirectory(absoluteFolder); 

        //Original name kept on entity only as metadata.
        var uuidFileName = $"{Guid.NewGuid():N}{AllowedExtension}";
        var absoluteFilePath = Path.Combine(absoluteFolder, uuidFileName);

        await using (var fileStream = new FileStream(
            absoluteFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await uploadStream.CopyToAsync(fileStream, ct);
        }

        var storagePath = $"{StorageFolderRelative}/{uuidFileName}";
        var safeOriginalName = Path.GetFileName(originalFileName); // strip any directory parts

        return (storagePath, safeOriginalName);
    }

    public string ResolveAbsolutePath(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path is required.", nameof(storagePath));

        var absolute = Path.GetFullPath(Path.Combine(_webRootPath, storagePath));
        var webRootCanonical = Path.GetFullPath(_webRootPath);

        if (!absolute.StartsWith(webRootCanonical, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Resolved path escapes the web root.");

        return absolute;
    }
}