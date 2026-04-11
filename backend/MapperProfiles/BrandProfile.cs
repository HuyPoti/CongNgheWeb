using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandDto>();
        
        CreateMap<CreateBrandDto, Brand>();
        
        CreateMap<UpdateBrandDto, Brand>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}