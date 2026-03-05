using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LinkUp.Application.DTOs.Response;
using LinkUp.Domain.Enums;

namespace LinkUp.Web.Models;

public class CreatePostViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El contenido es requerido")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tipo de media")]
    public string MediaType { get; set; } = "image";

    [Display(Name = "Imagen")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "URL de YouTube")]
    public string? YouTubeUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(MediaType, "video", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(YouTubeUrl))
                yield return new ValidationResult("Debe proporcionar un enlace de YouTube para publicaciones de tipo vídeo.", new[] { nameof(YouTubeUrl) });
            else if (!IsValidYouTubeUrl(YouTubeUrl))
                yield return new ValidationResult("El enlace debe ser de YouTube (youtube.com o youtu.be).", new[] { nameof(YouTubeUrl) });
        }
    }

    private static bool IsValidYouTubeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var u = url.Trim();
        return (u.Contains("youtube.com/watch", StringComparison.OrdinalIgnoreCase) && u.Contains("v=")) ||
               (u.Contains("youtu.be/", StringComparison.OrdinalIgnoreCase));
    }
}

public class EditPostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El contenido es requerido")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;

    public PostMediaType MediaType { get; set; }

    [Display(Name = "Nueva imagen")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "URL de YouTube")]
    public string? YouTubeUrl { get; set; }

    public string? CurrentImagePath { get; set; }
}

public class HomeViewModel
{
    public IEnumerable<PostResponseDto> Posts { get; set; } = new List<PostResponseDto>();
    public CreatePostViewModel NewPost { get; set; } = new CreatePostViewModel();
    public int CurrentUserId { get; set; }
    public string? CurrentUserProfilePicture { get; set; }
}

public class AddCommentViewModel
{
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }

    [Required(ErrorMessage = "El comentario no puede estar vacío")]
    public string Content { get; set; } = string.Empty;
}

public class EditCommentViewModel
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public string ReturnTo { get; set; } = "Index";

    [Required(ErrorMessage = "El comentario no puede estar vacío")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;
}
