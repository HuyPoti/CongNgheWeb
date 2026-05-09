using backend.DTOs;

namespace backend.Services;

public interface IShipmentService
{
    Task<ShipmentDto> CreateAsync(CreateShipmentDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> UpdateAsync(Guid shipmentId, UpdateShipmentDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> MarkQcPassedAsync(Guid shipmentId, MarkQcDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> MarkPackedAsync(Guid shipmentId, Guid userId, CancellationToken cancellationToken = default);
}
