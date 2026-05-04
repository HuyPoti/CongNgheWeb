using backend.DTOs;

namespace backend.Services;

public interface IReturnRequestService
{
    Task<IEnumerable<ReturnRequestDto>> GetAllAsync();
    Task<ReturnRequestDto?> GetByIdAsync(Guid id);
    Task<ReturnRequestDto?> GetByOrderIdAsync(Guid orderId);
    Task<ReturnRequestDto> CreateAsync(Guid userId, CreateReturnRequestDto dto);
    Task<ReturnRequestDto> ProcessAsync(Guid adminId, Guid returnId, UpdateReturnRequestDto dto);
}
