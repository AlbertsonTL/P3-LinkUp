using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LinkUp.Application.DTOs.Response;

namespace LinkUp.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido")]
    [RegularExpression(@"^(809|829|849)-\d{3}-\d{4}$", ErrorMessage = "El teléfono debe tener formato dominicano (ej: 809-123-4567)")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar contraseña es requerido")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Foto de perfil")]
    public IFormFile? ProfilePicture { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar contraseña es requerido")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido")]
    [RegularExpression(@"^(809|829|849)-\d{3}-\d{4}$", ErrorMessage = "Formato inválido (ej: 809-123-4567)")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña (dejar en blanco para no cambiar)")]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Foto de perfil")]
    public IFormFile? ProfilePicture { get; set; }

    public string? CurrentProfilePicture { get; set; }

    // Display-only data for the social-profile view — never bound from the
    // edit form (no [BindProperty]/form fields target these), populated by
    // AccountController from existing services on every GET/POST render.
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int FriendsCount { get; set; }
    public List<PostResponseDto> Posts { get; set; } = new();
}
