using AutoMapper;
using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class FriendRequestService : IFriendRequestService
{
    private readonly IFriendRequestRepository _requestRepo;
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public FriendRequestService(IFriendRequestRepository requestRepo, IFriendshipRepository friendshipRepo,
        UserManager<AppUser> userManager, IMapper mapper)
    {
        _requestRepo = requestRepo;
        _friendshipRepo = friendshipRepo;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FriendRequestResponseDto>> GetPendingRequestsAsync(int userId)
    {
        var requests = await _requestRepo.GetPendingRequestsForUserAsync(userId);
        var result = new List<FriendRequestResponseDto>();
        foreach (var req in requests)
        {
            var dto = _mapper.Map<FriendRequestResponseDto>(req);
            dto.CommonFriendsCount = await _friendshipRepo.GetCommonFriendsCountAsync(userId, req.SenderId);
            result.Add(dto);
        }
        return result;
    }

    public async Task<IEnumerable<FriendRequestResponseDto>> GetSentRequestsAsync(int userId)
    {
        var requests = await _requestRepo.GetSentRequestsByUserAsync(userId);
        var result = new List<FriendRequestResponseDto>();
        foreach (var req in requests)
        {
            var dto = _mapper.Map<FriendRequestResponseDto>(req);
            dto.CommonFriendsCount = await _friendshipRepo.GetCommonFriendsCountAsync(userId, req.ReceiverId);
            result.Add(dto);
        }
        return result;
    }

    public async Task<IEnumerable<UserResponseDto>> GetUsersForNewRequestAsync(int userId, string? search)
    {
        var friendIds = await _friendshipRepo.GetFriendIdsAsync(userId);
        var pendingRelated = (await _requestRepo.FindAsync(r =>
            (r.SenderId == userId || r.ReceiverId == userId) && r.Status == FriendRequestStatus.Pending))
            .SelectMany(r => new[] { r.SenderId, r.ReceiverId })
            .Distinct();

        var excludeIds = friendIds.Concat(pendingRelated).Append(userId).ToHashSet();

        var allUsers = _userManager.Users.Where(u => u.IsActive && !excludeIds.Contains(u.Id));
        if (!string.IsNullOrWhiteSpace(search))
            allUsers = allUsers.Where(u => u.UserName!.Contains(search));

        var result = new List<UserResponseDto>();
        foreach (var user in allUsers.Take(50).ToList())
        {
            var dto = _mapper.Map<UserResponseDto>(user);
            dto.CommonFriendsCount = await _friendshipRepo.GetCommonFriendsCountAsync(userId, user.Id);
            result.Add(dto);
        }
        return result;
    }

    public async Task<R> SendRequestAsync(int senderId, int receiverId)
    {
        var areFriends = await _friendshipRepo.AreFriendsAsync(senderId, receiverId);
        if (areFriends) return R.Failure("Ya son amigos.");

        var existingRequest = await _requestRepo.GetActiveRequestAsync(senderId, receiverId);
        if (existingRequest != null) return R.Failure("Ya existe una solicitud activa.");

        var reverseRequest = await _requestRepo.GetActiveRequestAsync(receiverId, senderId);
        if (reverseRequest != null) return R.Failure("Este usuario ya te envió una solicitud pendiente.");

        await _requestRepo.AddAsync(new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        return R.Success();
    }

    public async Task<R> AcceptRequestAsync(int requestId, int userId)
    {
        var req = await _requestRepo.GetByIdAsync(requestId);
        if (req == null || req.ReceiverId != userId)
            return R.Failure("Solicitud no encontrada.");

        req.Status = FriendRequestStatus.Accepted;
        await _requestRepo.UpdateAsync(req);

        await _friendshipRepo.AddAsync(new Friendship
        {
            User1Id = req.SenderId,
            User2Id = req.ReceiverId,
            CreatedAt = DateTime.UtcNow
        });

        return R.Success();
    }

    public async Task<R> RejectRequestAsync(int requestId, int userId)
    {
        var req = await _requestRepo.GetByIdAsync(requestId);
        if (req == null || req.ReceiverId != userId)
            return R.Failure("Solicitud no encontrada.");

        req.Status = FriendRequestStatus.Rejected;
        await _requestRepo.UpdateAsync(req);
        return R.Success();
    }

    public async Task<R> DeleteRequestAsync(int requestId, int userId)
    {
        var req = await _requestRepo.GetByIdAsync(requestId);
        if (req == null || req.SenderId != userId)
            return R.Failure("Solicitud no encontrada.");

        await _requestRepo.DeleteAsync(req);
        return R.Success();
    }
}
