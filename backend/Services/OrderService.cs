// using AutoMapper;
// using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
// using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService(
    IUnitOfWork uow) : IOrderService
{
    // CREATE ORDER
    public async Task<OrderDetailDto> CreateAsync(
        CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Items.Count == 0)
            throw new BadRequestException("Order must have at least one item");

        var userId = await ResolveUserIdAsync(dto.UserId, cancellationToken);
        var user = await uow.Users.Query()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User not found");

        var shippingAddress = await ResolveShippingAddressAsync(dto, userId, cancellationToken);

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await uow.Products.Query()
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        if (products.Count != productIds.Count)
            throw new BadRequestException("One or more products are invalid");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new BadRequestException("Item quantity must be greater than zero");

            var product = products[item.ProductId];
            if (product.Status != 2)
                throw new BadRequestException($"Product {product.Name} is not available");

            if (product.StockQuantity < item.Quantity)
                throw new BadRequestException($"Product {product.Name} does not have enough stock");

            var unitPrice = product.SalePrice.HasValue && product.SalePrice.Value > 0
                ? product.SalePrice.Value
                : product.RegularPrice;

            totalAmount += unitPrice * item.Quantity;
            product.StockQuantity -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            orderItems.Add(new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
            });
        }

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = userId,
            ShippingAddressId = shippingAddress.AddressId,
            OrderCode = GenerateOrderCode(),
            PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod)
                ? "cod"
                : dto.PaymentMethod,
            Notes = dto.Notes,
            TotalAmount = totalAmount,
            Status = 1,
            PaymentStatus = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = orderItems,
        };

        uow.Orders.Insert(order);
        await uow.SaveAsync(cancellationToken);

        var detail = await GetByIdAsync(order.OrderId, userId, cancellationToken);
        if (detail == null)
            throw new NotFoundException("Order created but cannot be loaded");

        return detail;
    }

    // GET ALL WITH PAGINATION + FILTER
    public async Task<PagedResult<OrderDto>> GetAllAsync(
        string? status,
        Guid? userId,
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

        // Filter by userId
        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            query = query.Where(o => o.UserId == userId.Value);
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
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = uow.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Images)
            .AsQueryable();

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var order = await query.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

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

    private static string GenerateOrderCode()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private async Task<Guid> ResolveUserIdAsync(Guid? requestedUserId, CancellationToken cancellationToken)
    {
        if (requestedUserId.HasValue && requestedUserId.Value != Guid.Empty)
        {
            return requestedUserId.Value;
        }

        var fallbackUserId = await uow.Users.Query()
            .Where(u => u.IsActive && u.Role == UserRole.customer)
            .OrderBy(u => u.CreatedAt)
            .Select(u => u.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fallbackUserId == Guid.Empty)
            throw new BadRequestException("No active customer account available for checkout");

        return fallbackUserId;
    }

    private async Task<Address> ResolveShippingAddressAsync(
        CreateOrderDto dto,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (dto.ShippingAddressId.HasValue && dto.ShippingAddressId.Value != Guid.Empty)
        {
            var existingAddress = await uow.Addresses.Query()
                .FirstOrDefaultAsync(
                    a => a.AddressId == dto.ShippingAddressId.Value && a.UserId == userId,
                    cancellationToken
                );

            if (existingAddress == null)
                throw new BadRequestException("Shipping address is invalid");

            return existingAddress;
        }

        if (dto.ShippingAddress == null)
            throw new BadRequestException("Shipping address is required");

        if (string.IsNullOrWhiteSpace(dto.ShippingAddress.RecipientName) ||
            string.IsNullOrWhiteSpace(dto.ShippingAddress.Phone) ||
            string.IsNullOrWhiteSpace(dto.ShippingAddress.AddressLine))
        {
            throw new BadRequestException("Shipping recipient name, phone and address are required");
        }

        var address = new Address
        {
            AddressId = Guid.NewGuid(),
            UserId = userId,
            RecipientName = dto.ShippingAddress.RecipientName.Trim(),
            Phone = dto.ShippingAddress.Phone.Trim(),
            AddressLine = dto.ShippingAddress.AddressLine.Trim(),
            Province = string.IsNullOrWhiteSpace(dto.ShippingAddress.Province)
                ? "-"
                : dto.ShippingAddress.Province.Trim(),
            District = string.IsNullOrWhiteSpace(dto.ShippingAddress.District)
                ? "-"
                : dto.ShippingAddress.District.Trim(),
            Ward = string.IsNullOrWhiteSpace(dto.ShippingAddress.Ward)
                ? "-"
                : dto.ShippingAddress.Ward.Trim(),
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
        };

        uow.Addresses.Insert(address);
        return address;
    }
}
