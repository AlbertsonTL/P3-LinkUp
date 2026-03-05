using AutoMapper;
using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Request;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepo;
    private readonly IPostReactionRepository _reactionRepo;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public PostService(IPostRepository postRepo, IPostReactionRepository reactionRepo,
        IMapper mapper, IFileStorageService fileStorage)
    {
        _postRepo = postRepo;
        _reactionRepo = reactionRepo;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<Result<PostResponseDto>> CreatePostAsync(int userId, CreatePostRequestDto dto)
    {
        var mediaType = dto.MediaTypeStr == "video" ? PostMediaType.Video : PostMediaType.Image;
        string? imagePath = null;

        if (mediaType == PostMediaType.Image)
        {
            if (dto.ImageData == null || dto.ImageData.Length == 0 || string.IsNullOrEmpty(dto.ImageFileName))
                return Result<PostResponseDto>.Failure("Debe subir una imagen para este tipo de publicación.");

            imagePath = await _fileStorage.SaveFileAsync(dto.ImageData, dto.ImageFileName, "posts");
        }

        if (mediaType == PostMediaType.Video && string.IsNullOrWhiteSpace(dto.YouTubeUrl))
            return Result<PostResponseDto>.Failure("Debe proporcionar un enlace de YouTube.");

        var post = new Post
        {
            Content = dto.Content,
            MediaType = mediaType,
            ImagePath = imagePath,
            YouTubeUrl = dto.YouTubeUrl,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _postRepo.AddAsync(post);
        var fullPost = await _postRepo.GetPostWithDetailsAsync(post.Id);
        return Result<PostResponseDto>.Success(_mapper.Map<PostResponseDto>(fullPost!));
    }

    public async Task<IEnumerable<PostResponseDto>> GetUserPostsAsync(int userId, int currentUserId)
    {
        var posts = await _postRepo.GetUserPostsAsync(userId);
        var result = new List<PostResponseDto>();
        foreach (var post in posts.OrderByDescending(p => p.CreatedAt))
        {
            var dto = _mapper.Map<PostResponseDto>(post);
            var reaction = await _reactionRepo.GetUserReactionAsync(post.Id, currentUserId);
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

    public async Task<R> UpdatePostAsync(int postId, int userId, UpdatePostRequestDto dto)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post == null || post.UserId != userId)
            return R.Failure("No tienes permiso para editar esta publicación.");

        post.Content = dto.Content;
        if (post.MediaType == PostMediaType.Video)
            post.YouTubeUrl = dto.YouTubeUrl;
        else if (dto.ImageData != null && dto.ImageData.Length > 0 && !string.IsNullOrEmpty(dto.ImageFileName))
            post.ImagePath = await _fileStorage.SaveFileAsync(dto.ImageData, dto.ImageFileName, "posts");

        post.UpdatedAt = DateTime.UtcNow;
        await _postRepo.UpdateAsync(post);
        return R.Success();
    }

    public async Task<R> DeletePostAsync(int postId, int userId)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post == null || post.UserId != userId)
            return R.Failure("No tienes permiso para eliminar esta publicación.");

        post.IsDeleted = true;
        await _postRepo.UpdateAsync(post);
        return R.Success();
    }

    public async Task<R> ReactToPostAsync(int postId, int userId, bool isLike)
    {
        var existing = await _reactionRepo.GetUserReactionAsync(postId, userId);
        var newType = isLike ? ReactionType.Like : ReactionType.Dislike;

        if (existing != null)
        {
            existing.ReactionType = newType;
            await _reactionRepo.UpdateAsync(existing);
        }
        else
        {
            await _reactionRepo.AddAsync(new PostReaction
            {
                PostId = postId, UserId = userId, ReactionType = newType
            });
        }
        return R.Success();
    }
}
