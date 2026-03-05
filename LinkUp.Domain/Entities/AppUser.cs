using Microsoft.AspNetCore.Identity;

namespace LinkUp.Domain.Entities;

public class AppUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public bool IsActive { get; set; } = false;
    public string? ActivationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    public virtual ICollection<FriendRequest> SentRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<FriendRequest> ReceivedRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<Friendship> FriendshipsAsUser1 { get; set; } = new List<Friendship>();
    public virtual ICollection<Friendship> FriendshipsAsUser2 { get; set; } = new List<Friendship>();
    public virtual ICollection<BattleshipGame> GamesAsPlayer1 { get; set; } = new List<BattleshipGame>();
    public virtual ICollection<BattleshipGame> GamesAsPlayer2 { get; set; } = new List<BattleshipGame>();
}
