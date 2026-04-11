using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class ProductSpecProfile : Profile
{
    public ProductSpecProfile()
    {
        CreateMap<ProductSpec, ProductSpecDto>();
        CreateMap<CreateProductSpecDto, ProductSpec>();
    }
}