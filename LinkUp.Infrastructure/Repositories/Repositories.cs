using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using LinkUp.Infrastructure.Persistence;
using LinkUp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace LinkUp.Infrastructure.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Post>> GetUserPostsAsync(int userId)
        => await _dbSet.Include(p => p.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.Replies.Where(r => !r.IsDeleted)).ThenInclude(r => r.User)
            .Include(p => p.Reactions)
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Post>> GetFriendPostsAsync(int userId)
    {
        var friendIds = await _context.Friendships
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync();

        return await _dbSet.Include(p => p.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.Replies.Where(r => !r.IsDeleted)).ThenInclude(r => r.User)
            .Include(p => p.Reactions)
            .Where(p => friendIds.Contains(p.UserId) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostWithDetailsAsync(int postId)
        => await _dbSet.Include(p => p.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.User)
            .Include(p => p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == postId);
}

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId)
        => await _dbSet.Include(c => c.User)
            .Include(c => c.Replies.Where(r => !r.IsDeleted)).ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public override async Task<Comment?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Comment, bool>> predicate)
        => await _dbSet.Include(c => c.User).Include(c => c.Replies).FirstOrDefaultAsync(predicate);
}

public class PostReactionRepository : GenericRepository<PostReaction>, IPostReactionRepository
{
    public PostReactionRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<PostReaction?> GetUserReactionAsync(int postId, int userId)
        => await _dbSet.FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
}

public class FriendRequestRepository : GenericRepository<FriendRequest>, IFriendRequestRepository
{
    public FriendRequestRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(int userId)
        => await _dbSet.Include(r => r.Sender).Include(r => r.Receiver)
            .Where(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(int userId)
        => await _dbSet.Include(r => r.Sender).Include(r => r.Receiver)
            .Where(r => r.SenderId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<FriendRequest?> GetActiveRequestAsync(int senderId, int receiverId)
        => await _dbSet.FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId && r.Status == FriendRequestStatus.Pending);
}

public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
{
    public FriendshipRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(int userId)
        => await _dbSet.Include(f => f.User1).Include(f => f.User2)
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .ToListAsync();

    public async Task<bool> AreFriendsAsync(int u1, int u2)
        => await _dbSet.AnyAsync(f => (f.User1Id == u1 && f.User2Id == u2) || (f.User1Id == u2 && f.User2Id == u1));

    public async Task<IEnumerable<int>> GetFriendIdsAsync(int userId)
        => await _dbSet.Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync();

    public async Task<int> GetCommonFriendsCountAsync(int userId1, int userId2)
    {
        var friends1 = await GetFriendIdsAsync(userId1);
        var friends2 = await GetFriendIdsAsync(userId2);
        return friends1.Intersect(friends2).Count();
    }
}

public class BattleshipGameRepository : GenericRepository<BattleshipGame>, IBattleshipGameRepository
{
    public BattleshipGameRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<BattleshipGame>> GetActiveGamesForUserAsync(int userId)
        => await _dbSet.Include(g => g.Player1).Include(g => g.Player2).Include(g => g.Winner)
            .Where(g => (g.Player1Id == userId || g.Player2Id == userId) && g.Status != GameStatus.Finished)
            .ToListAsync();

    public async Task<IEnumerable<BattleshipGame>> GetFinishedGamesForUserAsync(int userId)
        => await _dbSet.Include(g => g.Player1).Include(g => g.Player2).Include(g => g.Winner)
            .Where(g => (g.Player1Id == userId || g.Player2Id == userId) && g.Status == GameStatus.Finished)
            .OrderByDescending(g => g.FinishedAt)
            .ToListAsync();

    public async Task<BattleshipGame?> GetGameWithDetailsAsync(int gameId)
        => await _dbSet.Include(g => g.Player1).Include(g => g.Player2).Include(g => g.Winner)
            .Include(g => g.ShipPlacements).Include(g => g.Attacks)
            .FirstOrDefaultAsync(g => g.Id == gameId);

    public async Task<bool> HasActiveGameWithFriendAsync(int userId, int friendId)
        => await _dbSet.AnyAsync(g =>
            ((g.Player1Id == userId && g.Player2Id == friendId) || (g.Player1Id == friendId && g.Player2Id == userId))
            && g.Status != GameStatus.Finished);
}

public class ShipPlacementRepository : GenericRepository<ShipPlacement>, IShipPlacementRepository
{
    private static readonly int[] DefaultShipSizes = { 2, 3, 3, 4, 5 };

    public ShipPlacementRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<ShipPlacement>> GetUserShipsInGameAsync(int gameId, int userId)
        => await _dbSet.Where(s => s.GameId == gameId && s.UserId == userId).ToListAsync();

    public async Task<bool> AreAllShipsPlacedAsync(int gameId, int userId)
    {
        var ships = await GetUserShipsInGameAsync(gameId, userId);
        var placed = ships.Select(s => s.ShipSize).OrderBy(x => x).ToList();
        var required = DefaultShipSizes.OrderBy(x => x).ToList();
        return placed.SequenceEqual(required);
    }
}

public class AttackRepository : GenericRepository<Attack>, IAttackRepository
{
    public AttackRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Attack>> GetAttacksByGameAndAttackerAsync(int gameId, int attackerId)
        => await _dbSet.Where(a => a.GameId == gameId && a.AttackerId == attackerId).ToListAsync();

    public async Task<IEnumerable<Attack>> GetAllAttacksInGameAsync(int gameId)
        => await _dbSet.Where(a => a.GameId == gameId).ToListAsync();

    public async Task<bool> CellAlreadyAttackedAsync(int gameId, int attackerId, int row, int col)
        => await _dbSet.AnyAsync(a => a.GameId == gameId && a.AttackerId == attackerId && a.Row == row && a.Col == col);
}
