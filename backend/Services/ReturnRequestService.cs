using AutoMapper;
using backend.UnitOfWork;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Constants;

namespace backend.Services;

public class ReturnRequestService : IReturnRequestService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmailNotificationService _emailNotification;

    public ReturnRequestService(IUnitOfWork uow, IMapper mapper, IEmailNotificationService emailNotification)
    {
        _uow = uow;
        _mapper = mapper;
        _emailNotification = emailNotification;
    }

    public async Task<IEnumerable<ReturnRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _uow.ReturnRequests.Query()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ReturnRequestDto>>(requests);
    }

    public async Task<ReturnRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _uow.ReturnRequests.Query()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)

            .FirstOrDefaultAsync(r => r.ReturnId == id, cancellationToken);

        return _mapper.Map<ReturnRequestDto>(request);
    }

    public async Task<ReturnRequestDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var request = await _uow.ReturnRequests.Query()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)

            .FirstOrDefaultAsync(r => r.OrderId == orderId, cancellationToken);

        return _mapper.Map<ReturnRequestDto>(request);
    }

    public async Task<ReturnRequestDto> CreateAsync(Guid userId, CreateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra đơn hàng
        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId && o.UserId == userId, cancellationToken);

        if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        // 2. Kiểm tra trạng thái đơn hàng (Phải là 5 - Delivered)
        if (order.Status != OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Chỉ có thể đổi trả đơn hàng đã giao thành công.");
        }

        // 3. Kiểm tra thời hạn 7 ngày
        // Giả sử có trường DeliveredAt hoặc dùng UpdatedAt của Order
        var deliveryDate = order.UpdatedAt; // Cần kiểm tra lại logic lưu ngày giao hàng thực tế
        if (DateTime.UtcNow > deliveryDate.AddDays(7))
        {
            throw new InvalidOperationException("Đã quá thời hạn 7 ngày để yêu cầu đổi trả.");
        }

        // 4. Kiểm tra xem đã có yêu cầu nào chưa
        var existingRequest = await _uow.ReturnRequests.Query().AnyAsync(r => r.OrderId == dto.OrderId, cancellationToken);
        if (existingRequest)
        {
            throw new InvalidOperationException("Đơn hàng này đã có yêu cầu đổi trả.");
        }

        // 5. Tạo yêu cầu
        var returnRequest = new ReturnRequest
        {
            ReturnId = Guid.NewGuid(),
            OrderId = dto.OrderId,
            UserId = userId,
            Reason = dto.Reason,
            Description = dto.Description,
            Status = ReturnRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            returnRequest.Items.Add(new ReturnRequestItem
            {
                Id = Guid.NewGuid(),
                OrderItemId = itemDto.OrderItemId,
                Quantity = itemDto.Quantity,
                ReasonDetail = itemDto.ReasonDetail
            });
        }

        foreach (var imageUrl in dto.ImageUrls)
        {
            returnRequest.Images.Add(new ReturnRequestImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageUrl
            });
        }

        _uow.ReturnRequests.Insert(returnRequest);
        await _uow.SaveAsync(cancellationToken);
 
        return await GetByIdAsync(returnRequest.ReturnId, cancellationToken) ?? _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<ReturnRequestDto> ProcessAsync(Guid adminId, Guid returnId, UpdateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _uow.ReturnRequests.Query()
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem)
            .FirstOrDefaultAsync(r => r.ReturnId == returnId, cancellationToken);

        if (request == null) throw new KeyNotFoundException("Không tìm thấy yêu cầu đổi trả.");

        // Kiểm tra chuyển trạng thái hợp lệ
        var newStatus = dto.Status.ToLower();
        var currentStatus = request.Status.ToLower();

        var invalidTransitions = new Dictionary<string, HashSet<string>>
        {
            // Approved chỉ được chuyển sang Completed
            [ReturnRequestStatus.Approved] = new() { ReturnRequestStatus.Rejected },
            // Rejected không được chuyển sang trạng thái khác
            [ReturnRequestStatus.Rejected] = new() { ReturnRequestStatus.Approved, ReturnRequestStatus.Completed },
            // Completed là trạng thái cuối
            [ReturnRequestStatus.Completed] = new() { ReturnRequestStatus.Pending, ReturnRequestStatus.Approved, ReturnRequestStatus.Rejected },
        };

        if (invalidTransitions.TryGetValue(currentStatus, out var blocked) && blocked.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Không thể chuyển trạng thái từ '{currentStatus}' sang '{newStatus}'.");
        }

        var oldStatus = request.Status;
        request.Status = dto.Status;
        request.RefundAmount = dto.RefundAmount;
        request.AdminNote = dto.AdminNote;
        request.ProcessedBy = adminId;
        request.ProcessedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        // Logic Hoàn kho khi Approve
        if (oldStatus != ReturnRequestStatus.Approved && dto.Status == ReturnRequestStatus.Approved)
        {
            foreach (var item in request.Items)
            {
                if (item.OrderItem != null)
                {
                    var product = await _uow.Products.Query()
                        .FirstOrDefaultAsync(p => p.ProductId == item.OrderItem.ProductId, cancellationToken);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }
        }

        await _uow.SaveAsync(cancellationToken);

        return await GetByIdAsync(returnId, cancellationToken) ?? _mapper.Map<ReturnRequestDto>(request);
    }
}
