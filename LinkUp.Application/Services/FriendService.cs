using AutoMapper;
using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using Microsoft.AspNetCore.Identity;
using LinkUp.Domain.Entities;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class FriendService : IFriendService
{
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly IPostRepository _postRepo;
    private readonly IPostReactionRepository _reactionRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public FriendService(IFriendshipRepository friendshipRepo, IPostRepository postRepo,
        IPostReactionRepository reactionRepo, UserManager<AppUser> userManager, IMapper mapper)
    {
        _friendshipRepo = friendshipRepo;
        _postRepo = postRepo;
        _reactionRepo = reactionRepo;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FriendResponseDto>> GetFriendsAsync(int userId)
    {
        var friendships = await _friendshipRepo.GetUserFriendshipsAsync(userId);
        var result = new List<FriendResponseDto>();

        foreach (var f in friendships)
        {
            var friendId = f.User1Id == userId ? f.User2Id : f.User1Id;
            var friend = await _userManager.FindByIdAsync(friendId.ToString());
            if (friend == null) continue;

            var dto = _mapper.Map<FriendResponseDto>(friend);
            dto.CommonFriendsCount = await _friendshipRepo.GetCommonFriendsCountAsync(userId, friendId);
            result.Add(dto);
        }

        return result;
    }

    public async Task<R> RemoveFriendAsync(int userId, int friendId)
    {
        var friendships = await _friendshipRepo.GetUserFriendshipsAsync(userId);
        var friendship = friendships.FirstOrDefault(f =>
            (f.User1Id == userId && f.User2Id == friendId) ||
            (f.User1Id == friendId && f.User2Id == userId));

        if (friendship == null)
            return R.Failure("No son amigos.");

        await _friendshipRepo.DeleteAsync(friendship);
        return R.Success();
    }

    public async Task<IEnumerable<PostResponseDto>> GetFriendPublicationsAsync(int userId, int friendId)
    {
        var areFriends = await _friendshipRepo.AreFriendsAsync(userId, friendId);
        if (!areFriends)
            return Enumerable.Empty<PostResponseDto>();

        var posts = await _postRepo.GetUserPostsAsync(friendId);
        var result = new List<PostResponseDto>();

        foreach (var post in posts.OrderByDescending(p => p.CreatedAt))
        {
            var dto = _mapper.Map<PostResponseDto>(post);
            var reaction = await _reactionRepo.GetUserReactionAsync(post.Id, userId);
            dto.CurrentUserReaction = reaction?.ReactionType;
            result.Add(dto);
        }

        return result;
    }

    public async Task<IEnumerable<PostResponseDto>> GetFriendPostsAsync(int userId)
    {
        var posts = await _postRepo.GetFriendPostsAsync(userId);
        var result = new List<PostResponseDto>();
        foreach (var post in posts.OrderByDescending(p => p.CreatedAt))
        {
            var dto = _mapper.Map<PostResponseDto>(post);
            var reaction = await _reactionRepo.GetUserReactionAsync(post.Id, userId);
            dto.CurrentUserReaction = reaction?.ReactionType;
            result.Add(dto);
        }
        return result;
    }
}
