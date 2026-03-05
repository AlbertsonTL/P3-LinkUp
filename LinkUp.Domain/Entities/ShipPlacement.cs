using LinkUp.Domain.Base;
using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class ShipPlacement : BaseEntity
{
    public int GameId { get; set; }
    public int UserId { get; set; }
    public int ShipSize { get; set; }
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public ShipDirection Direction { get; set; }

    public virtual BattleshipGame Game { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
