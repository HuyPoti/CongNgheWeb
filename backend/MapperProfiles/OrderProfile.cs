using AutoMapper;
using backend.Models;
using backend.DTOs;
using backend.Constants;

namespace backend.MapperProfiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Order -> OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusToString(src.Status)))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => MapPaymentStatusToString(src.PaymentStatus)))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));

        // Order -> OrderDetailDto
        CreateMap<Order, OrderDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusToString(src.Status)))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => MapPaymentStatusToString(src.PaymentStatus)))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
            .ForMember(dest => dest.ReturnRequest, opt => opt.MapFrom(src => src.ReturnRequests.FirstOrDefault()));

        // OrderItem -> OrderItemDto
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : ""))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                src.Product != null && src.Product.Images != null && src.Product.Images.Any()
                    ? src.Product.Images.FirstOrDefault(i => i.IsPrimary) != null
                        ? src.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                        : src.Product.Images.FirstOrDefault()!.ImageUrl
                    : null
            ));

        // OrderStatusHistory -> OrderStatusHistoryDto
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
            .ForMember(dest => dest.OldStatusLabel, opt => opt.MapFrom(src => src.OldStatus.HasValue ? MapStatusToString(src.OldStatus.Value) : null))
            .ForMember(dest => dest.NewStatusLabel, opt => opt.MapFrom(src => MapStatusToString(src.NewStatus)))
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : "Hệ thống"));

        // Address -> AddressDto
        CreateMap<Address, AddressDto>();
    }

    private static string MapStatusToString(int status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Confirmed => "confirmed",
        OrderStatus.Processing => "processing",
        OrderStatus.Shipping => "shipping",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Cancelled => "cancelled",
        _ => "pending"
    };

    private static string MapPaymentStatusToString(int status) => status switch
    {
        PaymentStatus.Pending => "unpaid",
        PaymentStatus.Completed => "paid",
        PaymentStatus.Failed => "failed",
        PaymentStatus.Refunded => "refunded",
        _ => "unpaid"
    };
}
