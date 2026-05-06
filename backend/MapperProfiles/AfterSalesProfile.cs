using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class AfterSalesProfile : Profile
{
    public AfterSalesProfile()
    {
        // ReturnRequest Mappings
        CreateMap<ReturnRequest, ReturnRequestDto>()
            .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderCode : string.Empty))
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.ProcessedByName, opt => opt.MapFrom(src => src.ProcessedByUser != null ? src.ProcessedByUser.FullName : string.Empty));

        CreateMap<ReturnRequestItem, ReturnRequestItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem != null && src.OrderItem.Product != null ? src.OrderItem.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.OrderItem != null && src.OrderItem.Product != null && src.OrderItem.Product.Images.Any() ? src.OrderItem.Product.Images.First().ImageUrl : null))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.OrderItem != null ? src.OrderItem.UnitPrice : 0));

        CreateMap<ReturnRequestImage, ReturnRequestImageDto>();

        // Wishlist Mappings
        CreateMap<Wishlist, WishlistItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSlug, opt => opt.MapFrom(src => src.Product != null ? src.Product.Slug : string.Empty))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : null))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.RegularPrice : 0))
            .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.SalePrice : null))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Product != null ? src.Product.StockQuantity : 0));
    }
}
