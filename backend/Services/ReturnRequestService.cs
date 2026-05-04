using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ReturnRequestService : IReturnRequestService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ReturnRequestService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReturnRequestDto>> GetAllAsync()
    {
        var requests = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ReturnRequestDto>>(requests);
    }

    public async Task<ReturnRequestDto?> GetByIdAsync(Guid id)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)

            .FirstOrDefaultAsync(r => r.ReturnId == id);

        return _mapper.Map<ReturnRequestDto>(request);
    }

    public async Task<ReturnRequestDto?> GetByOrderIdAsync(Guid orderId)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Images)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Images)

            .FirstOrDefaultAsync(r => r.OrderId == orderId);

        return _mapper.Map<ReturnRequestDto>(request);
    }

    public async Task<ReturnRequestDto> CreateAsync(Guid userId, CreateReturnRequestDto dto)
    {
        // 1. Kiểm tra đơn hàng
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId && o.UserId == userId);

        if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        // 2. Kiểm tra trạng thái đơn hàng (Phải là 5 - Delivered)
        if (order.Status != 5)
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
        var existingRequest = await _context.ReturnRequests.AnyAsync(r => r.OrderId == dto.OrderId);
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
            Status = "pending",
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

        _context.ReturnRequests.Add(returnRequest);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(returnRequest.ReturnId) ?? _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<ReturnRequestDto> ProcessAsync(Guid adminId, Guid returnId, UpdateReturnRequestDto dto)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem)
            .FirstOrDefaultAsync(r => r.ReturnId == returnId);

        if (request == null) throw new KeyNotFoundException("Không tìm thấy yêu cầu đổi trả.");

        var oldStatus = request.Status;
        request.Status = dto.Status;
        request.RefundAmount = dto.RefundAmount;
        request.AdminNote = dto.AdminNote;
        request.ProcessedBy = adminId;
        request.ProcessedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        // Logic Hoàn kho khi Approve
        if (oldStatus != "approved" && dto.Status == "approved")
        {
            foreach (var item in request.Items)
            {
                if (item.OrderItem != null)
                {
                    var product = await _context.Products.FindAsync(item.OrderItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(returnId) ?? _mapper.Map<ReturnRequestDto>(request);
    }
}
