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
            .ForMember(dest => dest.IsSoldOut,
                opt => opt.MapFrom(src => src.SoldCount >= src.StockLimit));

        CreateMap<ActivityLog, ActivityLogDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));
    }
}
