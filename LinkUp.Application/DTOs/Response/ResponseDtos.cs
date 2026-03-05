using LinkUp.Domain.Enums;

namespace LinkUp.Application.DTOs.Response;

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public bool IsActive { get; set; }
    public int CommonFriendsCount { get; set; }
}

public class PostResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public PostMediaType MediaType { get; set; }
    public string? ImagePath { get; set; }
    public string? YouTubeUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfilePicture { get; set; }
    public List<CommentResponseDto> Comments { get; set; } = new();
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public ReactionType? CurrentUserReaction { get; set; }
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfilePicture { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentResponseDto> Replies { get; set; } = new();
}

public class FriendResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public int CommonFriendsCount { get; set; }
}

public class FriendRequestResponseDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public int ReceiverId { get; set; }
    public string ReceiverUserName { get; set; } = string.Empty;
    public string? ReceiverProfilePicture { get; set; }
    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CommonFriendsCount { get; set; }
}

public class BattleshipGameResponseDto
{
    public int Id { get; set; }
    public int Player1Id { get; set; }
    public string Player1UserName { get; set; } = string.Empty;
    public int Player2Id { get; set; }
    public string Player2UserName { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public int? CurrentTurnUserId { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public double HoursElapsed { get; set; }
    public bool Player1Ready { get; set; }
    public bool Player2Ready { get; set; }
}

public class PlacementBoardResponseDto
{
    public int GameId { get; set; }
    public int UserId { get; set; }
    public bool OtherPlayerReady { get; set; }
    public List<int[]> PlacedCells { get; set; } = new(); // [row, col]
    public List<ShipRemainingDto> RemainingShips { get; set; } = new();
}

public class ShipRemainingDto
{
    public int Size { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AttackBoardResponseDto
{
    public int GameId { get; set; }
    public int UserId { get; set; }
    public bool IsMyTurn { get; set; }
    public string? CurrentTurnUserName { get; set; }
    public List<AttackCellDto> AttackedCells { get; set; } = new();
    public GameStatus GameStatus { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerUserName { get; set; }
}

public class AttackCellDto
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsHit { get; set; }
}

public class GameResultResponseDto
{
    public int GameId { get; set; }
    public int RequestingUserId { get; set; }
    public int OpponentId { get; set; }
    public string OpponentUserName { get; set; } = string.Empty;
    public int? WinnerId { get; set; }
    public List<AttackCellDto> MyAttacks { get; set; } = new();
    public List<AttackCellDto> OpponentAttacks { get; set; } = new();
    public List<int[]> MyShipPlacements { get; set; } = new();
}
