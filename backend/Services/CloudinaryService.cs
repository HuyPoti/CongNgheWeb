using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, string? publicId = null)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File ảnh không hợp lệ.");

        const long maxFileSize = 2 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new Exception("Dung lượng ảnh vượt quá 2MB. Vui lòng chọn ảnh nhỏ hơn.");
        
        var allowedExtensions = new[] {".jpg", ".png", ".webp", ".gif", ".jpeg"};
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            throw new Exception("Chỉ hỗ trợ định dạng: JPG, PNG, WebP, GIF.");
        
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),

            // Dat ten thu muc tren Cloudinary (VD: "avatars/user_abc123")
            Folder = folder,
            // Tu dong nen + chuyen sang WebP (sieu nhe)
            // "auto:good" = can bang giua chat luong va dung luong
            Transformation = new Transformation()
                .Quality("auto:good")
                .FetchFormat("auto"),
            // Ghi de anh cu neu co PublicId (chong spam)
            // -> user upload 100 lan van chi 1 file tren Cloudinary
            Overwrite = publicId != null
        };

        if (!string.IsNullOrEmpty(publicId))
        {
            uploadParams.PublicId = publicId;
        }

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new Exception($"Loi upload anh: {result.Error.Message}");
        }

        // Tra ve URL da toi uu (HTTPS, nen, dung format)
        return result.SecureUrl.ToString();
    }
    
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}