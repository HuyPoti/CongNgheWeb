using System.Text.Json;
using backend.DTOs;
using backend.Services;

namespace backend.Extensions;

public static class ActivityLogExtensions
{
    public static async Task LogIfHasUserAsync(
        this IActivityLogService activityLogService,
        Guid? userId,
        string action,
        string entityType,
        Guid entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return;

        await activityLogService.LogAsync(
            new CreateActivityLogDto
            {
                UserId = userId.Value,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue == null ? null : JsonSerializer.Serialize(oldValue),
                NewValue = newValue == null ? null : JsonSerializer.Serialize(newValue)
            },
            cancellationToken);
    }
}
