using System.ComponentModel.DataAnnotations;
using LinkUp.Application.DTOs.Response;

namespace LinkUp.Web.Models;

// ViewModel para la pantalla principal de Amigos
public class FriendsIndexViewModel
{
    public IEnumerable<PostResponseDto> FriendPosts { get; set; } = new List<PostResponseDto>();
    public IEnumerable<FriendResponseDto> Friends { get; set; } = new List<FriendResponseDto>();
    public int CurrentUserId { get; set; }
    public string? CurrentUserProfilePicture { get; set; }
}

// ViewModel para las publicaciones de un amigo específico
public class FriendPublicationsViewModel
{
    public IEnumerable<PostResponseDto> Posts { get; set; } = new List<PostResponseDto>();
    public int FriendId { get; set; }
    public string FriendUserName { get; set; } = string.Empty;
    public string? FriendProfilePicture { get; set; }
    public int CurrentUserId { get; set; }
}

// ViewModel para confirmar la eliminación de un amigo
public class ConfirmRemoveFriendViewModel
{
    public int FriendId { get; set; }
    public string FriendUserName { get; set; } = string.Empty;
    public string? FriendProfilePicture { get; set; }
}
