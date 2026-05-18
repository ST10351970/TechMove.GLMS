namespace TechMove.GLMS.Core.Services;

// Handles validation, storage, and retrieval of uploaded files (PDF agreements).

public interface IFileService
{
    // Validates an upload (extension, content-type, size) without saving

    ValidationResult ValidateUpload(string? originalFileName, string? contentType, long sizeInBytes);

    /// <summary>
    /// Saves the uploaded stream to disk under a UUID filename.
    /// Returns the relative storage path and the sanitised original name.
    /// </summary>
    Task<(string StoragePath, string OriginalName)> SaveAsync(
        Stream uploadStream,
        string originalFileName,
        CancellationToken ct = default);

    // Resolves a stored relative path back to an absolute disk path for download.
    string ResolveAbsolutePath(string storagePath);
}