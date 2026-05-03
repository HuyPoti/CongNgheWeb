using backend.DTOs;

namespace backend.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken);
    Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
