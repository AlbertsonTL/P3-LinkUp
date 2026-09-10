using LinkUp.Application.DTOs.Response;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;

namespace LinkUp.Application.Mappings;

/// <summary>
/// Mapeos manuales entidad -> DTO que reemplazan al perfil de AutoMapper.
/// Cada método replica exactamente el comportamiento que tenía la configuración
/// correspondiente en el antiguo MappingProfile (incluyendo los campos que se
/// dejaban en su valor por defecto para ser rellenados luego por el servicio).
/// </summary>
public static class MappingExtensions
{
    // CreateMap<AppUser, UserResponseDto>()
    //     .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());
    public static UserResponseDto ToUserResponseDto(this AppUser user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            ProfilePicture = user.ProfilePicture,
            IsActive = user.IsActive
            // CommonFriendsCount se deja en su valor por defecto (0):
            // el servicio lo calcula y asigna aparte cuando corresponde.
        };
    }

    // CreateMap<AppUser, FriendResponseDto>()
    //     .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
    //     .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());
    public static FriendResponseDto ToFriendResponseDto(this AppUser user)
    {
        return new FriendResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePicture = user.ProfilePicture
            // CommonFriendsCount se deja en 0; se asigna aparte en el servicio.
        };
    }

    // CreateMap<Post, PostResponseDto>()
    //     .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.UserName))
    //     .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(s => s.User.ProfilePicture))
    //     .ForMember(d => d.LikeCount, opt => opt.MapFrom(s => s.Reactions.Count(r => r.ReactionType == ReactionType.Like)))
    //     .ForMember(d => d.DislikeCount, opt => opt.MapFrom(s => s.Reactions.Count(r => r.ReactionType == ReactionType.Dislike)))
    //     .ForMember(d => d.CurrentUserReaction, opt => opt.Ignore());
    public static PostResponseDto ToPostResponseDto(this Post post)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            Content = post.Content,
            MediaType = post.MediaType,
            ImagePath = post.ImagePath,
            YouTubeUrl = post.YouTubeUrl,
            CreatedAt = post.CreatedAt,
            UserId = post.UserId,
            UserName = post.User?.UserName ?? string.Empty,
            UserProfilePicture = post.User?.ProfilePicture,
            Comments = post.Comments?.Select(c => c.ToCommentResponseDto()).ToList() ?? new List<CommentResponseDto>(),
            LikeCount = post.Reactions?.Count(r => r.ReactionType == ReactionType.Like) ?? 0,
            DislikeCount = post.Reactions?.Count(r => r.ReactionType == ReactionType.Dislike) ?? 0
            // CurrentUserReaction se deja en null; el servicio lo asigna aparte.
        };
    }

    // CreateMap<Comment, CommentResponseDto>()
    //     .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.UserName))
    //     .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(s => s.User.ProfilePicture));
    // (AutoMapper mapeaba Replies recursivamente por convención al existir
    // el propio CreateMap<Comment, CommentResponseDto>)
    public static CommentResponseDto ToCommentResponseDto(this Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            Content = comment.Content,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = comment.User?.UserName ?? string.Empty,
            UserProfilePicture = comment.User?.ProfilePicture,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            Replies = comment.Replies?.Select(r => r.ToCommentResponseDto()).ToList() ?? new List<CommentResponseDto>()
        };
    }

    // CreateMap<FriendRequest, FriendRequestResponseDto>()
    //     .ForMember(d => d.SenderUserName, opt => opt.MapFrom(s => s.Sender.UserName))
    //     .ForMember(d => d.SenderProfilePicture, opt => opt.MapFrom(s => s.Sender.ProfilePicture))
    //     .ForMember(d => d.ReceiverUserName, opt => opt.MapFrom(s => s.Receiver.UserName))
    //     .ForMember(d => d.ReceiverProfilePicture, opt => opt.MapFrom(s => s.Receiver.ProfilePicture))
    //     .ForMember(d => d.CommonFriendsCount, opt => opt.Ignore());
    public static FriendRequestResponseDto ToFriendRequestResponseDto(this FriendRequest request)
    {
        return new FriendRequestResponseDto
        {
            Id = request.Id,
            SenderId = request.SenderId,
            SenderUserName = request.Sender?.UserName ?? string.Empty,
            SenderProfilePicture = request.Sender?.ProfilePicture,
            ReceiverId = request.ReceiverId,
            ReceiverUserName = request.Receiver?.UserName ?? string.Empty,
            ReceiverProfilePicture = request.Receiver?.ProfilePicture,
            Status = request.Status,
            CreatedAt = request.CreatedAt
            // CommonFriendsCount se deja en 0; se asigna aparte en el servicio.
        };
    }

    // CreateMap<BattleshipGame, BattleshipGameResponseDto>()
    //     .ForMember(d => d.Player1UserName, opt => opt.MapFrom(s => s.Player1.UserName))
    //     .ForMember(d => d.Player2UserName, opt => opt.MapFrom(s => s.Player2.UserName))
    //     .ForMember(d => d.WinnerUserName, opt => opt.MapFrom(s => s.Winner != null ? s.Winner.UserName : null))
    //     .ForMember(d => d.HoursElapsed, opt => opt.MapFrom(s =>
    //         (s.FinishedAt.HasValue ? s.FinishedAt.Value : DateTime.UtcNow).Subtract(s.CreatedAt).TotalHours));
    public static BattleshipGameResponseDto ToBattleshipGameResponseDto(this BattleshipGame game)
    {
        return new BattleshipGameResponseDto
        {
            Id = game.Id,
            Player1Id = game.Player1Id,
            Player1UserName = game.Player1?.UserName ?? string.Empty,
            Player2Id = game.Player2Id,
            Player2UserName = game.Player2?.UserName ?? string.Empty,
            Status = game.Status,
            CurrentTurnUserId = game.CurrentTurnUserId,
            WinnerId = game.WinnerId,
            WinnerUserName = game.Winner != null ? game.Winner.UserName : null,
            CreatedAt = game.CreatedAt,
            FinishedAt = game.FinishedAt,
            HoursElapsed = (game.FinishedAt ?? DateTime.UtcNow).Subtract(game.CreatedAt).TotalHours,
            Player1Ready = game.Player1Ready,
            Player2Ready = game.Player2Ready
        };
    }
}
