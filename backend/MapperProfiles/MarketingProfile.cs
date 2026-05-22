using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.MapperProfiles;

public class MarketingProfile : Profile
{
    public MarketingProfile()
    {
        CreateMap<Coupon, CouponDto>();
        CreateMap<CouponUsage, CouponUsageDto>();

        CreateMap<FlashSale, FlashSaleDto>();
        CreateMap<FlashSaleItem, FlashSaleItemDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.Slug,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Slug : string.Empty))
            .ForMember(dest => dest.ThumbnailUrl,
                opt => opt.MapFrom(src => src.Product != null ? 
                    (src.Product.Images.FirstOrDefault(i => i.IsPrimary) ?? src.Product.Images.FirstOrDefault())!.ImageUrl : null))
            .ForMember(dest => dest.RegularPrice,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.RegularPrice : 0))
            .ForMember(dest => dest.IsSoldOut,
                opt => opt.MapFrom(src => src.SoldCount >= src.StockLimit));

        CreateMap<ActivityLog, ActivityLogDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));
    }
}
