using LinkUp.Domain.Base;
using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class Post : AuditableEntity
{
    public string Content { get; set; } = string.Empty;
    public PostMediaType MediaType { get; set; }
    public string? ImagePath { get; set; }
    public string? YouTubeUrl { get; set; }
    public int UserId { get; set; }
    public bool IsDeleted { get; set; } = false;

    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
}
