namespace TechMove.GLMS.Services;

public class FileValidationService
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Returns true when fileName has a .pdf extension and fileSizeBytes is within the limit.
    /// Throws ArgumentNullException when fileName is null.
    /// Returns false for empty or non-PDF filenames, or oversized files.
    /// </summary>
    public bool IsValidFile(string? fileName, long fileSizeBytes = 0)
    {
        if (fileName is null)
            throw new ArgumentNullException(nameof(fileName), "File cannot be null.");
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
        if (!Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return false;
        if (fileSizeBytes > MaxFileSizeBytes)
            return false;
        return true;
    }
}
