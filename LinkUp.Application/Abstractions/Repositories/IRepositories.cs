using LinkUp.Domain.Entities;

namespace LinkUp.Application.Abstractions.Repositories;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<IEnumerable<Post>> GetUserPostsAsync(int userId);
    Task<IEnumerable<Post>> GetFriendPostsAsync(int userId);
    Task<Post?> GetPostWithDetailsAsync(int postId);
}

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId);
}

public interface IPostReactionRepository : IGenericRepository<PostReaction>
{
    Task<PostReaction?> GetUserReactionAsync(int postId, int userId);
}

public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
{
    Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(int userId);
    Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(int userId);
    Task<FriendRequest?> GetActiveRequestAsync(int senderId, int receiverId);
}

public interface IFriendshipRepository : IGenericRepository<Friendship>
{
    Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(int userId);
    Task<bool> AreFriendsAsync(int user1Id, int user2Id);
    Task<IEnumerable<int>> GetFriendIdsAsync(int userId);
    Task<int> GetCommonFriendsCountAsync(int userId1, int userId2);
}

public interface IBattleshipGameRepository : IGenericRepository<BattleshipGame>
{
    Task<IEnumerable<BattleshipGame>> GetActiveGamesForUserAsync(int userId);
    Task<IEnumerable<BattleshipGame>> GetFinishedGamesForUserAsync(int userId);
    Task<BattleshipGame?> GetGameWithDetailsAsync(int gameId);
    Task<bool> HasActiveGameWithFriendAsync(int userId, int friendId);
}

public interface IShipPlacementRepository : IGenericRepository<ShipPlacement>
{
    Task<IEnumerable<ShipPlacement>> GetUserShipsInGameAsync(int gameId, int userId);
    Task<bool> AreAllShipsPlacedAsync(int gameId, int userId);
}

public interface IAttackRepository : IGenericRepository<Attack>
{
    Task<IEnumerable<Attack>> GetAttacksByGameAndAttackerAsync(int gameId, int attackerId);
    Task<IEnumerable<Attack>> GetAllAttacksInGameAsync(int gameId);
    Task<bool> CellAlreadyAttackedAsync(int gameId, int attackerId, int row, int col);
}
