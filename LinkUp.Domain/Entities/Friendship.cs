using LinkUp.Domain.Base;

namespace LinkUp.Domain.Entities;

public class Friendship : BaseEntity
{
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual AppUser User1 { get; set; } = null!;
    public virtual AppUser User2 { get; set; } = null!;
}
