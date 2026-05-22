using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Models;
using backend.UnitOfWork;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using backend.Constants;
using backend.Extensions;
using backend.Exceptions;

namespace backend.Services;

public class OrderService(
    IUnitOfWork uow,
    IMapper mapper,
    IEmailNotificationService emailNotification,
    IFlashSaleService flashSaleService,
    ICouponService couponService) : IOrderService
{
    // CREATE ORDER
    public async Task<OrderDetailDto> CreateAsync(
        CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
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
            throw new NotFoundException("One or more products are invalid");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            var product = products[item.ProductId];
            if (product.Status != ProductStatus.Published)
                throw new BadRequestException($"Product {product.Name} is not available");

            if (product.StockQuantity < item.Quantity)
                throw new BadRequestException($"Product {product.Name} has insufficient stock");

            var flashPrice = await flashSaleService.GetFlashPriceAsync(product.ProductId, cancellationToken);
            var unitPrice = flashPrice ?? (product.SalePrice.HasValue && product.SalePrice.Value > 0
                ? product.SalePrice.Value
                : product.RegularPrice);

            if (flashPrice.HasValue)
            {
                var reserved = await flashSaleService.RecordPurchaseAsync(product.ProductId, item.Quantity, cancellationToken);
                if (!reserved)
                    throw new BadRequestException($"Flash sale for product {product.Name} has ended or reached limit.");
            }

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

        Guid? couponId = null;
        decimal discountAmount = 0;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var validationItems = orderItems.Select(oi => new CouponValidationItemDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity
            }).ToList();

            var validation = await couponService.ValidateAsync(
                dto.CouponCode,
                totalAmount,
                userId,
                validationItems,
                cancellationToken);

            if (!validation.IsValid)
            {
                throw new BadRequestException(validation.Message);
            }

            couponId = validation.CouponId;
            discountAmount = validation.DiscountAmount;
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
            TotalAmount = Math.Max(totalAmount - discountAmount, 0),
            CouponId = couponId,
            DiscountAmount = discountAmount,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = orderItems,
        };

        uow.Orders.Insert(order);

        uow.OrderStatusHistories.Insert(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.OrderId,
            OldStatus = null,
            NewStatus = OrderStatus.Pending,
            ChangedBy = userId,
            Note = "Đơn hàng mới tạo",
            CreatedAt = DateTime.UtcNow
        });

        await uow.SaveAsync(cancellationToken);

        if (couponId.HasValue)
        {
            var coupon = await uow.Coupons.Query().FirstOrDefaultAsync(c => c.CouponId == couponId.Value, cancellationToken);
            if (coupon != null)
            {
                coupon.UsedCount += 1;
                
                var usage = new CouponUsage
                {
                    Id = Guid.NewGuid(),
                    CouponId = coupon.CouponId,
                    UserId = userId,
                    OrderId = order.OrderId,
                    DiscountAmount = discountAmount,
                    UsedAt = DateTime.UtcNow
                };
                uow.CouponUsages.Insert(usage);
                await uow.SaveAsync(cancellationToken);
            }
        }

        // Gửi email xác nhận đơn hàng
        await emailNotification.SendOrderConfirmedEmail(order.OrderId);

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

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
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
            .Include(o => o.ReturnRequests)
            .AsQueryable();

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var order = await query.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null) return null;

        return mapper.Map<OrderDetailDto>(order);
    }

    // UPDATE STATUS
    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateOrderDto dto,
        Guid changedByUserId,
        CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order not found");

        // Update status if provided
        if (!string.IsNullOrEmpty(dto.Status))
        {
            var newStatus = MapStatusToInt(dto.Status);
            if (newStatus != order.Status)
            {
                var oldStatus = order.Status;
                order.Status = newStatus;

                // Create history
                uow.OrderStatusHistories.Insert(new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedBy = changedByUserId,
                    Note = "Trạng thái cập nhật từ quản trị",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Update payment status if provided
        if (!string.IsNullOrEmpty(dto.PaymentStatus))
        {
            order.PaymentStatus = MapPaymentStatusToInt(dto.PaymentStatus);
        }

        order.UpdatedAt = DateTime.UtcNow;

        uow.Orders.Update(order);
        await uow.SaveAsync(cancellationToken);

        // Gửi email thông báo trạng thái mới
        if (order.Status == OrderStatus.Shipping) // shipping
        {
            await emailNotification.SendOrderShippingEmail(order.OrderId);
        }
        else if (order.Status == OrderStatus.Delivered) // delivered
        {
            await emailNotification.SendOrderDeliveredEmail(order.OrderId);
        }

        return true;
    }

    // CANCEL ORDER
    public async Task<bool> CancelAsync(
        Guid id,
        CancelOrderDto dto,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order not found");

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            throw new BadRequestException("Order is already delivered or cancelled.");

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.CancelledReason = dto.Reason;
        order.CancelledBy = userId;
        order.UpdatedAt = DateTime.UtcNow;

        // Hoàn lại stock [R5]
        var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
        var products = await uow.Products.Query()
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        foreach (var item in order.OrderItems)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                product.StockQuantity += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                uow.Products.Update(product);
            }
        }

        // Add History
        uow.OrderStatusHistories.Insert(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.OrderId,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Cancelled,
            ChangedBy = userId ?? Guid.Empty,
            Note = $"Đã hủy đơn hàng. Lý do: {dto.Reason}",
            CreatedAt = DateTime.UtcNow
        });

        uow.Orders.Update(order);
        await uow.SaveAsync(cancellationToken);

        return true;
    }

    // GET STATUS HISTORY
    public async Task<List<OrderStatusHistoryDto>> GetStatusHistoryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await uow.OrderStatusHistories.Query()
            .Include(h => h.ChangedByUser)
            .Where(h => h.OrderId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<OrderStatusHistoryDto>>(history);
    }

    // Helper: Map int → string
    private static string MapStatusToString(int status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Confirmed => "confirmed",
        OrderStatus.Processing => "processing",
        OrderStatus.Shipping => "shipping",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Cancelled => "cancelled",
        _ => "pending"
    };

    private static int MapStatusToInt(string status) => status.ToLower() switch
    {
        "pending" => OrderStatus.Pending,
        "confirmed" => OrderStatus.Confirmed,
        "processing" => OrderStatus.Processing,
        "shipping" => OrderStatus.Shipping,
        "delivered" => OrderStatus.Delivered,
        "cancelled" => OrderStatus.Cancelled,
        _ => OrderStatus.Pending
    };

    private static string MapPaymentStatusToString(int status) => status switch
    {
        PaymentStatus.Pending => "unpaid",
        PaymentStatus.Completed => "paid",
        PaymentStatus.Failed => "failed",
        PaymentStatus.Refunded => "refunded",
        _ => "unpaid"
    };

    private static int MapPaymentStatusToInt(string status) => status.ToLower() switch
    {
        "unpaid" => PaymentStatus.Pending,
        "paid" => PaymentStatus.Completed,
        "failed" => PaymentStatus.Failed,
        "refunded" => PaymentStatus.Refunded,
        _ => PaymentStatus.Pending
    };

    private static string GenerateOrderCode()
    {
        return $"ORD-{DateTime.UtcNow:yyMMddHHmmssfff}";
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

