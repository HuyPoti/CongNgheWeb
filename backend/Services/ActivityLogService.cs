using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public interface IActivityLogService
{
	Task<ActivityLogDto> LogAsync(CreateActivityLogDto dto, CancellationToken cancellationToken = default);
	Task<PagedResult<ActivityLogDto>> GetLogsAsync(ActivityLogQueryDto query, CancellationToken cancellationToken = default);
}

public class ActivityLogService(AppDbContext context, IMapper mapper) : IActivityLogService
{
	public async Task<ActivityLogDto> LogAsync(
		CreateActivityLogDto dto,
		CancellationToken cancellationToken = default)
	{
		if (dto.UserId == Guid.Empty)
			throw new BadRequestException("UserId is required to create an activity log");

		if (string.IsNullOrWhiteSpace(dto.Action))
			throw new BadRequestException("Action is required");

		var log = new ActivityLog
		{
			LogId = Guid.NewGuid(),
			UserId = dto.UserId,
			Action = dto.Action.Trim(),
			EntityType = dto.EntityType,
			EntityId = dto.EntityId,
			OldValue = dto.OldValue,
			NewValue = dto.NewValue,
			IpAddress = dto.IpAddress,
			CreatedAt = DateTime.UtcNow
		};

		context.ActivityLogs.Add(log);
		await context.SaveChangesAsync(cancellationToken);

		var saved = await context.ActivityLogs
			.Include(x => x.User)
			.FirstAsync(x => x.LogId == log.LogId, cancellationToken);

		return mapper.Map<ActivityLogDto>(saved);
	}

	public async Task<PagedResult<ActivityLogDto>> GetLogsAsync(
		ActivityLogQueryDto query,
		CancellationToken cancellationToken = default)
	{
		var page = query.Page <= 0 ? 1 : query.Page;
		var pageSize = Math.Clamp(query.PageSize, 1, 100);

		var logsQuery = context.ActivityLogs
			.Include(x => x.User)
			.AsQueryable();

		if (query.UserId.HasValue && query.UserId.Value != Guid.Empty)
			logsQuery = logsQuery.Where(x => x.UserId == query.UserId.Value);

		if (!string.IsNullOrWhiteSpace(query.EntityType))
		{
			var entityType = query.EntityType.Trim();
			logsQuery = logsQuery.Where(x => x.EntityType != null && EF.Functions.ILike(x.EntityType, entityType));
		}

		if (query.FromDate.HasValue)
			logsQuery = logsQuery.Where(x => x.CreatedAt >= query.FromDate.Value);

		if (query.ToDate.HasValue)
			logsQuery = logsQuery.Where(x => x.CreatedAt <= query.ToDate.Value);

		var totalCount = await logsQuery.CountAsync(cancellationToken);

		var items = await logsQuery
			.OrderByDescending(x => x.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<ActivityLogDto>
		{
			Items = mapper.Map<List<ActivityLogDto>>(items),
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		};
	}
}
