using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using backend.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductSpecService(IUnitOfWork uow, IMapper mapper) : IProductSpecService
{
    public async Task<List<ProductSpecDto>> GetByProductIdAsync(Guid productId, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync<ProductDto>(productId, ct);
        if (product == null) throw new NotFoundException("Product not found");

        var specs = await uow.ProductSpecs.Query()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.SpecKey)
            .ToListAsync(ct);

        return mapper.Map<List<ProductSpecDto>>(specs);
    }
    public async Task<ProductSpecDto> AddAsync(Guid productId, CreateProductSpecDto dto, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync<ProductDto>(productId, ct);
        if (product == null) throw new NotFoundException("Product not found");

        var spec = mapper.Map<ProductSpec>(dto);
        spec.ProductId = productId;
        spec.CreatedAt = DateTime.UtcNow;

        uow.ProductSpecs.Insert(spec);
        await uow.SaveAsync(ct);

        return mapper.Map<ProductSpecDto>(spec);
    }

    public async Task<ProductSpecDto> UpdateAsync(Guid specId, CreateProductSpecDto dto, CancellationToken ct)
    {
        var spec = await uow.ProductSpecs.Query().FirstOrDefaultAsync(s => s.SpecId == specId, ct);
        if (spec == null) throw new NotFoundException("Spec not found");

        spec.SpecKey = dto.SpecKey;
        spec.SpecValue = dto.SpecValue;

        uow.ProductSpecs.Update(spec);
        await uow.SaveAsync(ct);

        return mapper.Map<ProductSpecDto>(spec);
    }

    public async Task DeleteAsync(Guid specId, CancellationToken ct)
    {
        var deleted = await uow.ProductSpecs.DeleteAsync(specId, ct);
        if (deleted == null) throw new NotFoundException("Spec not found");
        await uow.SaveAsync(ct);
    }
}