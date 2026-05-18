using FluentAssertions;
using TechMove.GLMS.Core.Services;
using Xunit;

namespace TechMove.GLMS.Tests.FileHandling;

public class FileServiceTests
{
    private readonly FileService _service = new(Path.GetTempPath());

    [Fact]
    public void ValidateUpload_ValidPdf_ReturnsSuccess()
    {
        var result = _service.ValidateUpload(
            originalFileName: "agreement.pdf",
            contentType: "application/pdf",
            sizeInBytes: 1024);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("malware.exe")]
    [InlineData("script.js")]
    [InlineData("photo.jpg")]
    [InlineData("doc.docx")]
    [InlineData("noextension")]
    public void ValidateUpload_NonPdfExtension_ReturnsFailure(string fileName)
    {
        //.exe file rejection
        var result = _service.ValidateUpload(
            originalFileName: fileName,
            contentType: "application/pdf",
            sizeInBytes: 1024);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("PDF");
    }

    [Fact]
    public void ValidateUpload_PdfRenamedFromExe_StillRejectedByContentType()
    {
        var result = _service.ValidateUpload(
            originalFileName: "malware.pdf",
            contentType: "application/x-msdownload",
            sizeInBytes: 1024);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("content type");
    }

    [Fact]
    public void ValidateUpload_EmptyFile_ReturnsFailure()
    {
        // Edge case: zero-byte upload
        var result = _service.ValidateUpload(
            originalFileName: "agreement.pdf",
            contentType: "application/pdf",
            sizeInBytes: 0);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("empty");
    }

    [Fact]
    public void ValidateUpload_OversizedFile_ReturnsFailure()
    {
        // Edge case: > 10 MB
        var result = _service.ValidateUpload(
            originalFileName: "agreement.pdf",
            contentType: "application/pdf",
            sizeInBytes: 11 * 1024 * 1024);

        result.IsValid.Should().BeFalse();
        result.ErrorSummary.Should().Contain("MB");
    }

    [Fact]
    public void ValidateUpload_NullFileName_ReturnsFailure()
    {
        // Edge case: null inputs
        var result = _service.ValidateUpload(
            originalFileName: null,
            contentType: "application/pdf",
            sizeInBytes: 1024);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateUpload_PdfExtensionUppercase_IsAccepted()
    {
        // Real-world edge case
        var result = _service.ValidateUpload(
            originalFileName: "AGREEMENT.PDF",
            contentType: "application/pdf",
            sizeInBytes: 1024);

        result.IsValid.Should().BeTrue();
    }
}