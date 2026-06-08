using TechMove.GLMS.Services;

namespace TechMove.GLMS.Tests;

public class FileValidationTests
{
    private readonly FileValidationService _sut = new();

    [Fact]
    public void PdfFile_IsValid_ReturnsTrue()
    {
        var result = _sut.IsValidFile("signed-agreement.pdf", 1024);

        Assert.True(result);
    }

    [Fact]
    public void ExeFile_IsInvalid_ReturnsFalse()
    {
        var result = _sut.IsValidFile("malware.exe", 1024);

        Assert.False(result);
    }

    [Fact]
    public void DocxFile_IsInvalid_ReturnsFalse()
    {
        var result = _sut.IsValidFile("contract.docx", 1024);

        Assert.False(result);
    }

    [Fact]
    public void NullFile_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.IsValidFile(null));
    }

    [Fact]
    public void EmptyFileName_ReturnsFalse()
    {
        var result = _sut.IsValidFile(string.Empty);

        Assert.False(result);
    }

    [Fact]
    public void WhitespaceFileName_ReturnsFalse()
    {
        var result = _sut.IsValidFile("   ", 1024);

        Assert.False(result);
    }

    [Fact]
    public void FileSizeExactlyAtLimit_IsValid()
    {
        // Boundary: exactly 10 MB is allowed (limit check uses >, not >=)
        var result = _sut.IsValidFile("doc.pdf", FileValidationService.MaxFileSizeBytes);

        Assert.True(result);
    }

    [Fact]
    public void FileSizeOverLimit_ReturnsFalse()
    {
        // One byte over the 10 MB limit must be rejected
        var result = _sut.IsValidFile("doc.pdf", FileValidationService.MaxFileSizeBytes + 1);

        Assert.False(result);
    }

    [Fact]
    public void UppercasePdfExtension_IsValid()
    {
        // Extension check is case-insensitive — .PDF must be treated the same as .pdf
        var result = _sut.IsValidFile("DOC.PDF", 1024);

        Assert.True(result);
    }
}
