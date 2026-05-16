using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using backend.Constants;

namespace backend.Services;

public class ShipmentService(IUnitOfWork uow) : IShipmentService
{
    // POST /api/shipments
    public async Task<ShipmentDto> CreateAsync(CreateShipmentDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Validate order exists and is confirmed
        var order = await uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order not found");

        if (order.Status != OrderStatus.Confirmed) // must be confirmed
            throw new BadRequestException("Order must be in 'confirmed' status before creating a shipment");

        // Check no active shipment already
        var existing = await uow.Shipments.Query()
            .AnyAsync(s => s.OrderId == dto.OrderId, cancellationToken);
        if (existing)
            throw new BadRequestException("A shipment already exists for this order");

        var shipment = new Shipment
        {
            ShipmentId = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Carrier = dto.Carrier.Trim(),
            TrackingCode = dto.TrackingCode?.Trim(),
            ShippingFee = dto.ShippingFee,
            EstimatedDelivery = dto.EstimatedDelivery,
            Status = ShipmentStatus.Packing,
            QcPassed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        uow.Shipments.Insert(shipment);

        // Move order to processing [R2]
        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
        uow.Orders.Update(order);

        // Log status history [R6]
        uow.OrderStatusHistories.Insert(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.OrderId,
            OldStatus = OrderStatus.Confirmed,
            NewStatus = OrderStatus.Processing,
            ChangedBy = createdByUserId,
            Note = $"Tạo phiếu giao hàng, hãng vận chuyển: {dto.Carrier}",
            CreatedAt = DateTime.UtcNow
        });

        await uow.SaveAsync(cancellationToken);

        return await MapToDto(shipment, cancellationToken);
    }

    // PUT /api/shipments/{id}
    public async Task<ShipmentDto> UpdateAsync(Guid shipmentId, UpdateShipmentDto dto, CancellationToken cancellationToken = default)
    {
        var shipment = await uow.Shipments.Query()
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken)
            ?? throw new NotFoundException("Shipment not found");

        if (dto.Carrier != null) shipment.Carrier = dto.Carrier.Trim();
        if (dto.TrackingCode != null) shipment.TrackingCode = dto.TrackingCode.Trim();
        if (dto.ShippingFee.HasValue) shipment.ShippingFee = dto.ShippingFee.Value;
        if (dto.EstimatedDelivery.HasValue) shipment.EstimatedDelivery = dto.EstimatedDelivery;
        if (dto.ActualDelivery.HasValue) shipment.ActualDelivery = dto.ActualDelivery;

        // If tracking code provided AND QC passed → auto move to shipping [R3]
        if (!string.IsNullOrWhiteSpace(shipment.TrackingCode) && shipment.QcPassed)
        {
            if (shipment.Status != ShipmentStatus.Shipping)
            {
                shipment.Status = ShipmentStatus.Shipping;

                var order = await uow.Orders.Query()
                    .FirstOrDefaultAsync(o => o.OrderId == shipment.OrderId, cancellationToken);
                if (order != null && order.Status != OrderStatus.Shipping && order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Cancelled) // Not delivered, cancelled or already shipping
                {
                    int oldStatus = order.Status;
                    order.Status = OrderStatus.Shipping; // Shipping
                    order.UpdatedAt = DateTime.UtcNow;
                    uow.Orders.Update(order);

                    // Log history
                    uow.OrderStatusHistories.Insert(new OrderStatusHistory
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        OldStatus = oldStatus,
                        NewStatus = OrderStatus.Shipping,
                        Note = $"Đã cập nhật mã vận đơn: {shipment.TrackingCode}. Đơn hàng chuyển sang 'Đang giao'.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        shipment.UpdatedAt = DateTime.UtcNow;
        uow.Shipments.Update(shipment);
        await uow.SaveAsync(cancellationToken);

        return await MapToDto(shipment, cancellationToken);
    }

    // GET /api/shipments/order/{orderId}
    public async Task<ShipmentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var shipment = await uow.Shipments.Query()
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

        if (shipment == null) return null;
        return await MapToDto(shipment, cancellationToken);
    }

    // PATCH /api/shipments/{id}/qc
    public async Task<ShipmentDto> MarkQcPassedAsync(Guid shipmentId, MarkQcDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await uow.Shipments.Query()
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken)
            ?? throw new NotFoundException("Shipment not found");

        shipment.QcPassed = dto.QcPassed;
        shipment.QcNotes = dto.QcNotes;
        if (dto.QcPassed) shipment.Status = ShipmentStatus.QcPassed;
        shipment.UpdatedAt = DateTime.UtcNow;

        uow.Shipments.Update(shipment);
        await uow.SaveAsync(cancellationToken);
        return await MapToDto(shipment, cancellationToken);
    }

    // PATCH /api/shipments/{id}/packed
    public async Task<ShipmentDto> MarkPackedAsync(Guid shipmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await uow.Shipments.Query()
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken)
            ?? throw new NotFoundException("Shipment not found");

        shipment.PackedBy = userId;
        shipment.PackedAt = DateTime.UtcNow;
        shipment.Status = ShipmentStatus.Packed;
        shipment.UpdatedAt = DateTime.UtcNow;

        uow.Shipments.Update(shipment);
        await uow.SaveAsync(cancellationToken);
        return await MapToDto(shipment, cancellationToken);
    }

    // Helper map
    private async Task<ShipmentDto> MapToDto(Shipment shipment, CancellationToken cancellationToken)
    {
        string? packedByName = null;
        if (shipment.PackedBy.HasValue)
        {
            var user = await uow.Users.Query()
                .Where(u => u.UserId == shipment.PackedBy.Value)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);
            packedByName = user;
        }

        return new ShipmentDto
        {
            ShipmentId = shipment.ShipmentId,
            OrderId = shipment.OrderId,
            Carrier = shipment.Carrier,
            TrackingCode = shipment.TrackingCode,
            ShippingFee = shipment.ShippingFee,
            EstimatedDelivery = shipment.EstimatedDelivery,
            ActualDelivery = shipment.ActualDelivery,
            Status = shipment.Status,
            QcPassed = shipment.QcPassed,
            QcNotes = shipment.QcNotes,
            PackedBy = shipment.PackedBy,
            PackedByName = packedByName,
            PackedAt = shipment.PackedAt,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt
        };
    }
}
