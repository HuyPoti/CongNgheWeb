using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SupplierService(IUnitOfWork uow, IMapper mapper) : ISupplierService
{
    public async Task<List<SupplierDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await uow.Suppliers.Query()
            .Where(s => s.IsActive)
            .ProjectTo<SupplierDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await uow.Suppliers.GetByIdAsync<SupplierDto>(id, cancellationToken);
        return supplier;
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Supplier>(dto);
        uow.Suppliers.Insert(entity);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<SupplierDto>(entity);
    }

    public async Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken)
    {
        var entity = await uow.Suppliers.Query().FirstOrDefaultAsync(s => s.SupplierId == id, cancellationToken);
        if (entity == null) return null;

        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Suppliers.Update(entity);
        await uow.SaveAsync(cancellationToken);
        
        return mapper.Map<SupplierDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await uow.Suppliers.Query().FirstOrDefaultAsync(s => s.SupplierId == id, cancellationToken);
        if (entity == null) return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Suppliers.Update(entity);
        await uow.SaveAsync(cancellationToken);
        
        return true;
    }
}
