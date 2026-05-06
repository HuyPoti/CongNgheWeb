using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.MapperProfiles;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<UpdateSupplierDto, Supplier>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
