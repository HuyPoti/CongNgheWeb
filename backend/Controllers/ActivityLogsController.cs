using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null, [FromQuery] string? entityType = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = new ActivityLogQueryDto
        {
            Page = page,
            PageSize = pageSize,
            UserId = userId,
            EntityType = entityType,
            FromDate = from,
            ToDate = to
        };

        var res = await _activityLogService.GetLogsAsync(query, cancellationToken);
        return Ok(res);
    }
}
