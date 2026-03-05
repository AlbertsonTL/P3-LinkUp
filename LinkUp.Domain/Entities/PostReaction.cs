using LinkUp.Domain.Base;
using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class PostReaction : BaseEntity
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Post Post { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
