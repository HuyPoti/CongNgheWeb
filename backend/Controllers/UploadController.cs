using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
    public async Task<ActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var  userId = GetUserId();

            var imageUrl = await cloudinaryService.UploadImageAsync(file, folder: "avatars", publicId: $"user_{userId}");

            var dto = new DTOs.UpdateProfileDto {AvatarUrl = imageUrl};
            var updatedUser = await profileService.UpdateProfileAsync(userId, dto, cancellationToken);
            
            return Ok(new
            {
                imageUrl,
                user = updatedUser
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    } 

    [Authorize]
    [HttpPost("{folder}")]
    public async Task<ActionResult> UploadImage(string folder, IFormFile file)
    {
        try
        {
            var allowedFolders = new[] { "products", "banners", "news","reviews"};
            if (!allowedFolders.Contains(folder.ToLower()))
                return BadRequest(new {message = $"Folder '{folder}' khong hop le. "});

            var imageUrl = await cloudinaryService.UploadImageAsync(file, folder);

            return Ok(new {imageUrl});
        }
        catch (Exception ex){
            return BadRequest(new {message = ex.Message});
        }
    }

    /// <summary>
    /// Xoa anh khoi Cloudinary (dung cho Admin hoac khi xoa product).
    /// </summary>
    [Authorize]
    [HttpDelete]
    public async Task<ActionResult> DeleteImage([FromQuery] string publicId)
    {
        try
        {
            var success = await cloudinaryService.DeleteImageAsync(publicId);
            if (!success)
                return NotFound(new { message = "Khong tim thay anh de xoa." });
            return Ok(new { message = "Da xoa anh thanh cong." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(idStr!);
    }
}