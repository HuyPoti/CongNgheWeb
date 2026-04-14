// using AutoMapper;
// using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Exceptions;
// using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService(
    IUnitOfWork uow) : IOrderService
{
    // GET ALL WITH PAGINATION + FILTER
    public async Task<PagedResult<OrderDto>> GetAllAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        pageSize = pageSize > 20 ? 20 : pageSize;

        var query = uow.Orders.Query()
            .Include(o => o.User)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            var statusInt = MapStatusToInt(status);
            query = query.Where(o => o.Status == statusInt);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                Status = MapStatusToString(o.Status),
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = MapPaymentStatusToString(o.PaymentStatus),
                TotalAmount = o.TotalAmount,
                CustomerName = o.User.FullName,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // GET BY ID (Order detail)
    public async Task<OrderDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null) return null;

        return new OrderDetailDto
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            Status = MapStatusToString(order.Status),
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = MapPaymentStatusToString(order.PaymentStatus),
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            UserId = order.UserId,
            CustomerName = order.User.FullName,
            ShippingAddress = new AddressDto
            {
                AddressId = order.Address.AddressId,
                RecipientName = order.Address.RecipientName,
                Phone = order.Address.Phone,
                AddressLine = order.Address.AddressLine,
                Province = order.Address.Province,
                District = order.Address.District,
                Ward = order.Address.Ward
            },
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                ProductId = oi.ProductId,
                ProductName = oi.Product != null ? oi.Product.Name : "",
                ProductImageUrl = oi.Product != null && oi.Product.Images.Any()
                    ? oi.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                      ?? oi.Product.Images.FirstOrDefault()?.ImageUrl!
                    : null,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    // UPDATE STATUS
    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order not found");

        // Update status if provided
        if (!string.IsNullOrEmpty(dto.Status))
        {
            order.Status = MapStatusToInt(dto.Status);
        }

        // Update payment status if provided
        if (!string.IsNullOrEmpty(dto.PaymentStatus))
        {
            order.PaymentStatus = MapPaymentStatusToInt(dto.PaymentStatus);
        }

        order.UpdatedAt = DateTime.UtcNow;

        uow.Orders.Update(order);
        await uow.SaveAsync(cancellationToken);

        return true;
    }

    // Helper: Map int → string
    private static string MapStatusToString(int status) => status switch
    {
        1 => "pending",
        2 => "confirmed",
        3 => "processing",
        4 => "shipping",
        5 => "delivered",
        6 => "cancelled",
        _ => "pending"
    };

    private static int MapStatusToInt(string status) => status.ToLower() switch
    {
        "pending" => 1,
        "confirmed" => 2,
        "processing" => 3,
        "shipping" => 4,
        "delivered" => 5,
        "cancelled" => 6,
        _ => 1
    };

    private static string MapPaymentStatusToString(int status) => status switch
    {
        1 => "unpaid",
        2 => "paid",
        3 => "refunded",
        _ => "unpaid"
    };

    private static int MapPaymentStatusToInt(string status) => status.ToLower() switch
    {
        "unpaid" => 1,
        "paid" => 2,
        "refunded" => 3,
        _ => 1
    };
}
