using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class NewsCategoryProfile : Profile
{
    public NewsCategoryProfile()
    {
        CreateMap<NewsCategory, NewsCategoryDto>();
        
        CreateMap<CreateNewsCategoryDto, NewsCategory>();
        
        CreateMap<UpdateNewsCategoryDto, NewsCategory>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}