using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(idStr!);
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetProfileAsync(GetUserId(), cancellationToken);
        if (profile == null) return NotFound(new { message = "Không tìm thấy user." });
        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var profile = await profileService.UpdateProfileAsync(GetUserId(), dto, ct);
        if (profile == null) return NotFound(new { message = "Không tìm thấy user." });
        return Ok(profile);
    }
}