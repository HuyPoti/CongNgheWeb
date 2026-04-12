using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using backend.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductImageService(IUnitOfWork uow, IMapper mapper) : IProductImageService
{
    public async Task<List<ProductImageDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        var exists = await uow.Products.GetByIdAsync<ProductDto>(productId, cancellationToken);
        if (exists == null)
            throw new NotFoundException("Product not found");

        var images = await uow.ProductImages.Query()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ProductImageDto>>(images);
    }

    public async Task<ProductImageDto> AddAsync(Guid productId, CreateProductImageDto dto, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync<ProductDto>(productId, ct);
        if (product == null) throw new NotFoundException("Product not found");

        var image = mapper.Map<ProductImage>(dto);
        image.ProductId = productId;
        image.CreatedAt = DateTime.UtcNow;

        if (dto.IsPrimary)
        {
            var oldPrimary = await uow.ProductImages.Query().Where(x => x.ProductId == productId && x.IsPrimary).ToListAsync(ct);
            foreach (var img in oldPrimary) img.IsPrimary = false;
        }

        uow.ProductImages.Insert(image);
        await uow.SaveAsync(ct);

        return mapper.Map<ProductImageDto>(image);
    }

    public async Task DeleteAsync(Guid ImageId, CancellationToken ct)
    {
        var deleted = await uow.ProductImages.DeleteAsync(ImageId, ct);
        if (deleted == null) throw new NotFoundException("Image not found");
        await uow.SaveAsync(ct);
    }
}