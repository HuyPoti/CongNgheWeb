using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.MapperProfiles;

public class InventoryProfile : Profile
{
    public InventoryProfile()
    {
        CreateMap<InventoryReceipt, InventoryReceiptDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null));
            
        CreateMap<InventoryReceiptItem, InventoryReceiptItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : null));
            
        CreateMap<CreateInventoryReceiptDto, InventoryReceipt>();
        CreateMap<CreateInventoryReceiptItemDto, InventoryReceiptItem>();

        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
    }
}
