using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>();
        
        CreateMap<Payment, PaymentTransactionDto>()
            .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.Order.OrderCode))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Order.User.FullName));
    }
}
