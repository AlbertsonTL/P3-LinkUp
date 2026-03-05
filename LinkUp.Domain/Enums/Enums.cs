namespace LinkUp.Domain.Enums;

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public enum ReactionType
{
    Like = 0,
    Dislike = 1
}

public enum PostMediaType
{
    Image = 0,
    Video = 1
}

public enum GameStatus
{
    WaitingPlacement = 0,
    InProgress = 1,
    Finished = 2
}

public enum ShipDirection
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public enum ShipSize
{
    Size2 = 2,
    Size3 = 3,
    Size4 = 4,
    Size5 = 5
}
