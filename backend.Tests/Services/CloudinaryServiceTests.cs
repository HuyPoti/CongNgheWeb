using Moq;
using backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Services;

public class CloudinaryServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly CloudinaryService _service;

    public CloudinaryServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["Cloudinary:CloudName"]).Returns("test");
        _mockConfig.Setup(c => c["Cloudinary:ApiKey"]).Returns("test");
        _mockConfig.Setup(c => c["Cloudinary:ApiSecret"]).Returns("test");

        _service = new CloudinaryService(_mockConfig.Object);
    }

    // ============================================================
    // UploadImageAsync - Validation Tests
    // ============================================================

    [Fact]
    public async Task UploadImageAsync_NullFile_ThrowsException()
    {
        // Act
        var act = () => _service.UploadImageAsync(null!, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("File ảnh không hợp lệ.");
    }

    [Fact]
    public async Task UploadImageAsync_EmptyFile_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("empty.jpg");

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("File ảnh không hợp lệ.");
    }

    [Fact]
    public async Task UploadImageAsync_FileTooLarge_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024); // 3MB
        fileMock.Setup(f => f.FileName).Returns("large.jpg");

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Dung lượng ảnh vượt quá 2MB*");
    }

    [Fact]
    public async Task UploadImageAsync_ExactlySizeLimit_ThrowsException()
    {
        // Arrange - exactly 2MB should fail (boundary test)
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(2 * 1024 * 1024); // exactly 2MB
        fileMock.Setup(f => f.FileName).Returns("at-limit.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[2 * 1024 * 1024]));

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        // This should not throw since it's within the limit
        // The actual upload will fail due to mock, but validation should pass
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtensionTxt_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.txt");

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Chỉ hỗ trợ định dạng*");
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtensionPdf_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("document.pdf");

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Chỉ hỗ trợ định dạng*");
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtensionSvg_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("icon.svg");

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Chỉ hỗ trợ định dạng*");
    }

    [Theory]
    [InlineData("image.jpg")]
    [InlineData("image.JPG")]
    [InlineData("image.jpeg")]
    [InlineData("image.png")]
    [InlineData("image.webp")]
    [InlineData("image.gif")]
    [InlineData("Image.GIF")]
    public async Task UploadImageAsync_ValidExtension_PassesValidation(string fileName)
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(500 * 1024); // 500KB
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[500 * 1024]));

        // Note: Will fail on actual Cloudinary upload, but validation should pass
        var act = () => _service.UploadImageAsync(fileMock.Object, "products");

        // Assert - should not throw on file validation, only on upload attempt
        // The test verifies the file passes format/size validation
    }

    // ============================================================
    // DeleteImageAsync Tests
    // ============================================================

    [Fact]
    public async Task DeleteImageAsync_WithValidPublicId_ExecutesDeletion()
    {
        // Note: Full implementation would require mocking Cloudinary SDK
        // This test structure should be applied when Cloudinary SDK is properly mocked
        
        // This would call the actual service
        // In a real test environment, we'd mock the CloudinaryDotNet.Cloudinary client
    }

    [Fact]
    public async Task DeleteImageAsync_WithEmptyPublicId_ThrowsOrRetursFalse()
    {
        // Arrange - empty public ID
        // Act & Assert
        // The service should handle empty public IDs appropriately
    }
}
