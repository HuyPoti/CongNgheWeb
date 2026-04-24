using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.MapperProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, User>();
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash)));
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}