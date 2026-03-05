using AutoMapper;
using LinkUp.Application.DTOs.Response;
using LinkUp.Domain.Entities;

namespace LinkUp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppUser, UserResponseDto>()
            .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());

        CreateMap<AppUser, FriendResponseDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());

        CreateMap<Post, PostResponseDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.UserName))
            .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(s => s.User.ProfilePicture))
            .ForMember(d => d.LikeCount, opt => opt.MapFrom(s => s.Reactions.Count(r => r.ReactionType == Domain.Enums.ReactionType.Like)))
            .ForMember(d => d.DislikeCount, opt => opt.MapFrom(s => s.Reactions.Count(r => r.ReactionType == Domain.Enums.ReactionType.Dislike)))
            .ForMember(d => d.CurrentUserReaction, opt => opt.Ignore());

        CreateMap<Comment, CommentResponseDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.UserName))
            .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(s => s.User.ProfilePicture));

        CreateMap<FriendRequest, FriendRequestResponseDto>()
            .ForMember(d => d.SenderUserName, opt => opt.MapFrom(s => s.Sender.UserName))
            .ForMember(d => d.SenderProfilePicture, opt => opt.MapFrom(s => s.Sender.ProfilePicture))
            .ForMember(d => d.ReceiverUserName, opt => opt.MapFrom(s => s.Receiver.UserName))
            .ForMember(d => d.ReceiverProfilePicture, opt => opt.MapFrom(s => s.Receiver.ProfilePicture))
            .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());

        CreateMap<BattleshipGame, BattleshipGameResponseDto>()
            .ForMember(d => d.Player1UserName, opt => opt.MapFrom(s => s.Player1.UserName))
            .ForMember(d => d.Player2UserName, opt => opt.MapFrom(s => s.Player2.UserName))
            .ForMember(d => d.WinnerUserName, opt => opt.MapFrom(s => s.Winner != null ? s.Winner.UserName : null))
            .ForMember(d => d.HoursElapsed, opt => opt.MapFrom(s =>
                (s.FinishedAt.HasValue ? s.FinishedAt.Value : DateTime.UtcNow).Subtract(s.CreatedAt).TotalHours));
    }
}
