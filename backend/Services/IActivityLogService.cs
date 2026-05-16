using backend.DTOs;

namespace backend.Services;

public interface IActivityLogService
{
	Task<ActivityLogDto> LogAsync(CreateActivityLogDto dto, CancellationToken cancellationToken = default);
	Task<PagedResult<ActivityLogDto>> GetLogsAsync(ActivityLogQueryDto query, CancellationToken cancellationToken = default);
}
