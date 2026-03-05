using LinkUp.Application.DTOs.Request;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Enums;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Abstractions.Services;

public interface IAccountService
{
    Task<Result> RegisterAsync(RegisterRequestDto dto, string activationBaseUrl);
    Task<Result<UserResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<Result> ActivateAccountAsync(string token);
    Task<Result> ForgotPasswordAsync(string username, string resetBaseUrl);
    Task<Result> ResetPasswordAsync(string token, string newPassword);
    Task<Result> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto);
    Task<UserResponseDto?> GetUserByIdAsync(int userId);
}

public interface IPostService
{
    Task<Result<PostResponseDto>> CreatePostAsync(int userId, CreatePostRequestDto dto);
    Task<IEnumerable<PostResponseDto>> GetUserPostsAsync(int userId, int currentUserId);
    Task<IEnumerable<PostResponseDto>> GetFriendPostsAsync(int userId);
    Task<Result> UpdatePostAsync(int postId, int userId, UpdatePostRequestDto dto);
    Task<Result> DeletePostAsync(int postId, int userId);
    Task<Result> ReactToPostAsync(int postId, int userId, bool isLike);
}

public interface ICommentService
{
    Task<Result<CommentResponseDto>> AddCommentAsync(int postId, int userId, string content, int? parentCommentId);
    Task<Result> UpdateCommentAsync(int commentId, int userId, string content);
    Task<Result> DeleteCommentAsync(int commentId, int userId);
}

public interface IFriendService
{
    Task<IEnumerable<FriendResponseDto>> GetFriendsAsync(int userId);
    Task<Result> RemoveFriendAsync(int userId, int friendId);
    Task<IEnumerable<PostResponseDto>> GetFriendPublicationsAsync(int userId, int friendId);
    Task<IEnumerable<PostResponseDto>> GetFriendPostsAsync(int userId);
}

public interface IFriendRequestService
{
    Task<IEnumerable<FriendRequestResponseDto>> GetPendingRequestsAsync(int userId);
    Task<IEnumerable<FriendRequestResponseDto>> GetSentRequestsAsync(int userId);
    Task<IEnumerable<UserResponseDto>> GetUsersForNewRequestAsync(int userId, string? search);
    Task<Result> SendRequestAsync(int senderId, int receiverId);
    Task<Result> AcceptRequestAsync(int requestId, int userId);
    Task<Result> RejectRequestAsync(int requestId, int userId);
    Task<Result> DeleteRequestAsync(int requestId, int userId);
}

public interface IBattleshipService
{
    Task<IEnumerable<BattleshipGameResponseDto>> GetActiveGamesAsync(int userId);
    Task<IEnumerable<BattleshipGameResponseDto>> GetFinishedGamesAsync(int userId);
    Task<IEnumerable<FriendResponseDto>> GetFriendsForNewGameAsync(int userId);
    Task<Result<int>> CreateGameAsync(int userId, int friendId);
    Task<Result> SurrenderAsync(int gameId, int userId);
    Task<PlacementBoardResponseDto> GetPlacementBoardAsync(int gameId, int userId);
    Task<Result> PlaceShipAsync(int gameId, int userId, int shipSize, int startRow, int startCol, ShipDirection direction);
    Task<AttackBoardResponseDto> GetAttackBoardAsync(int gameId, int userId);
    Task<Result> AttackAsync(int gameId, int userId, int row, int col);
    Task<GameResultResponseDto> GetGameResultAsync(int gameId, int userId);
    Task CheckAbandonedGamesAsync();
}
