using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class NewsProfile : Profile
{
    public NewsProfile()
    {
        CreateMap<News, NewsDto>()
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.Author != null ? s.Author.FullName : string.Empty));

        CreateMap<CreateNewsDto, News>();
        CreateMap<UpdateNewsDto, News>()
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}