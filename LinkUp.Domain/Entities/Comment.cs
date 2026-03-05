using LinkUp.Domain.Base;

namespace LinkUp.Domain.Entities;

public class Comment : AuditableEntity
{
    public string Content { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int UserId { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsDeleted { get; set; } = false;

    public virtual Post Post { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
