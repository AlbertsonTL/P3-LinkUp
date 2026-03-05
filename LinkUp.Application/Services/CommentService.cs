using AutoMapper;
using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Entities;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly IMapper _mapper;

    public CommentService(ICommentRepository commentRepo, IMapper mapper)
    {
        _commentRepo = commentRepo;
        _mapper = mapper;
    }

    public async Task<Result<CommentResponseDto>> AddCommentAsync(int postId, int userId, string content, int? parentCommentId)
    {
        var comment = new Comment
        {
            Content = content,
            PostId = postId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };
        await _commentRepo.AddAsync(comment);

        // Reload with user info
        var loaded = await _commentRepo.FirstOrDefaultAsync(c => c.Id == comment.Id);
        return Result<CommentResponseDto>.Success(_mapper.Map<CommentResponseDto>(loaded!));
    }

    public async Task<R> UpdateCommentAsync(int commentId, int userId, string content)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment == null || comment.UserId != userId)
            return R.Failure("No tienes permiso para editar este comentario.");

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentRepo.UpdateAsync(comment);
        return R.Success();
    }

    public async Task<R> DeleteCommentAsync(int commentId, int userId)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment == null || comment.UserId != userId)
            return R.Failure("No tienes permiso para eliminar este comentario.");

        comment.IsDeleted = true;
        await _commentRepo.UpdateAsync(comment);
        return R.Success();
    }
}
