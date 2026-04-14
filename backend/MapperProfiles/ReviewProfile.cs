using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.MapperProfiles;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.HelpfulVoteCount, opt => opt.MapFrom(src => src.HelpfulVotes.Count));

        CreateMap<ReviewImage, ReviewImageDto>();
        
        CreateMap<ReviewReply, ReviewReplyDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));

        CreateMap<ReviewHelpfulVote, ReviewHelpfulVoteDto>()
            .ForMember(dest => dest.UserName, opt=> opt.MapFrom(src => src.User.FullName));
    }
}
