using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class BannerProfile : Profile
{
    public BannerProfile()
    {
        CreateMap<Banner, BannerDto>();
        
        CreateMap<CreateBannerDto, Banner>();
        
        CreateMap<UpdateBannerDto, Banner>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}