using LinkUp.Domain.Base;
using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class BattleshipGame : AuditableEntity
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public GameStatus Status { get; set; } = GameStatus.WaitingPlacement;
    public int? CurrentTurnUserId { get; set; }
    public int? WinnerId { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool Player1Ready { get; set; } = false;
    public bool Player2Ready { get; set; } = false;

    public virtual AppUser Player1 { get; set; } = null!;
    public virtual AppUser Player2 { get; set; } = null!;
    public virtual AppUser? Winner { get; set; }
    public virtual ICollection<ShipPlacement> ShipPlacements { get; set; } = new List<ShipPlacement>();
    public virtual ICollection<Attack> Attacks { get; set; } = new List<Attack>();
}
