namespace backend.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, string? publicId = null);
    Task<bool> DeleteImageAsync(string publicId);
}