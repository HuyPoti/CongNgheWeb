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

    [Fact]
    public async Task UploadImageAsync_NullFile_ThrowsException()
    {
        // Act
        var act = () => _service.UploadImageAsync(null!, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("File ảnh không hợp lệ.");
    }

    [Fact]
    public async Task UploadImageAsync_FileTooLarge_ThrowsException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024); // 3MB

        // Act
        var act = () => _service.UploadImageAsync(fileMock.Object, "folder");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Dung lượng ảnh vượt quá 2MB*");
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtension_ThrowsException()
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
}
