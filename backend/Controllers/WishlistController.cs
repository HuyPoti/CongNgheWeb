using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _service;

    public WishlistController(IWishlistService service)
    {
        _service = service;
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        return Guid.Parse(idStr!);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WishlistItemDto>>>> GetMyWishlist()
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetByUserAsync(userId);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("my-ids")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Guid>>>> GetMyWishlistIds()
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetMyWishlistIdsAsync(userId);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("toggle/{productId}")]
    public async Task<ActionResult<ApiResponse<object>>> Toggle(Guid productId)
    {
        var userId = GetCurrentUserId();
        var isAdded = await _service.ToggleAsync(userId, productId);
        return Ok(ApiResponse.Ok(new { 
            isAdded, 
            message = isAdded ? "Đã thêm vào danh sách yêu thích" : "Đã xóa khỏi danh sách yêu thích" 
        }));
    }

    [HttpGet("check/{productId}")]
    public async Task<ActionResult<ApiResponse<object>>> Check(Guid productId)
    {
        var userId = GetCurrentUserId();
        var isInWishlist = await _service.IsInWishlistAsync(userId, productId);
        return Ok(ApiResponse.Ok(new { isInWishlist }));
    }
}
