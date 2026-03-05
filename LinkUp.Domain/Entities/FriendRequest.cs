using LinkUp.Domain.Base;
using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class FriendRequest : AuditableEntity
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

    public virtual AppUser Sender { get; set; } = null!;
    public virtual AppUser Receiver { get; set; } = null!;
}
