using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadController(
    ICloudinaryService cloudinaryService,
    IProfileService profileService
) : ControllerBase
{
    [Authorize]
    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<object>>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var  userId = GetUserId();

        var imageUrl = await cloudinaryService.UploadImageAsync(file, folder: "avatars", publicId: $"user_{userId}");

        var dto = new DTOs.UpdateProfileDto {AvatarUrl = imageUrl};
        var updatedUser = await profileService.UpdateProfileAsync(userId, dto, cancellationToken);
        
        return Ok(ApiResponse.Ok(new
        {
            imageUrl,
            user = updatedUser
        }));
    } 

    [Authorize]
    [HttpPost("{folder}")]
    public async Task<ActionResult<ApiResponse<object>>> UploadImage(string folder, IFormFile file)
    {
        var allowedFolders = new[] { "products", "banners", "news", "reviews", "returns", "brands", "categories" };
        if (!allowedFolders.Contains(folder.ToLower()))
            return BadRequest(ApiResponse.Fail($"Folder '{folder}' khong hop le. "));

        var imageUrl = await cloudinaryService.UploadImageAsync(file, folder);

        return Ok(ApiResponse.Ok(new {imageUrl}));
    }

    /// <summary>
    /// Xoa anh khoi Cloudinary (dung cho Admin hoac khi xoa product).
    /// </summary>
    [Authorize(Roles = "admin,staff")]
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage([FromQuery] string publicId)
    {
        var success = await cloudinaryService.DeleteImageAsync(publicId);
        if (!success)
            return NotFound(ApiResponse.Fail("Khong tim thay anh de xoa."));
        return Ok(ApiResponse.Ok(new { message = "Da xoa anh thanh cong." }));
    }

    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(idStr!);
    }
}