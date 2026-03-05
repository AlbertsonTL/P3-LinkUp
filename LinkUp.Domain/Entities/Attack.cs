using LinkUp.Domain.Base;

namespace LinkUp.Domain.Entities;

public class Attack : BaseEntity
{
    public int GameId { get; set; }
    public int AttackerId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsHit { get; set; }
    public DateTime AttackedAt { get; set; } = DateTime.UtcNow;

    public virtual BattleshipGame Game { get; set; } = null!;
    public virtual AppUser Attacker { get; set; } = null!;
}
