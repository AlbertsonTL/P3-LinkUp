using AutoMapper;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Request;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Entities;
using LinkUp.Shared.Emails;
using Microsoft.AspNetCore.Identity;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;
    private readonly IFileStorageService _fileStorage;

    public AccountService(UserManager<AppUser> userManager, IMapper mapper,
        IEmailSender emailSender, IFileStorageService fileStorage)
    {
        _userManager = userManager;
        _mapper = mapper;
        _emailSender = emailSender;
        _fileStorage = fileStorage;
    }

    public async Task<R> RegisterAsync(RegisterRequestDto dto, string activationBaseUrl)
    {
        var existing = await _userManager.FindByNameAsync(dto.UserName);
        if (existing != null)
            return R.Failure("El nombre de usuario ya está en uso.");

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
            return R.Failure("El correo electrónico ya está registrado.");

        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            ProfilePicture = dto.ProfilePicturePath,
            IsActive = false,
            ActivationToken = Guid.NewGuid().ToString("N")
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return R.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        var activationLink = $"{activationBaseUrl}?token={user.ActivationToken}";
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Activa tu cuenta en LinkUp ✓",
            EmailTemplateService.AccountActivation(user.FirstName, activationLink));

        return R.Success();
    }

    public async Task<Result<UserResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            return Result<UserResponseDto>.Failure("Usuario o contraseña incorrectos.");

        if (!user.IsActive)
            return Result<UserResponseDto>.Failure(
                "Tu cuenta no está activa. Revisa tu correo electrónico para activarla.");

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            return Result<UserResponseDto>.Failure("Usuario o contraseña incorrectos.");

        return Result<UserResponseDto>.Success(_mapper.Map<UserResponseDto>(user));
    }

    public async Task<R> ActivateAccountAsync(string token)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.ActivationToken == token);
        if (user == null)
            return R.Failure("Token de activación inválido.");

        user.IsActive = true;
        user.ActivationToken = null;
        await _userManager.UpdateAsync(user);
        return R.Success();
    }

    public async Task<R> ForgotPasswordAsync(string username, string resetBaseUrl)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return R.Failure("No se encontró un usuario con ese nombre.");

        user.IsActive = false;
        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userManager.UpdateAsync(user);

        var resetLink = $"{resetBaseUrl}?token={user.PasswordResetToken}";
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Restablecer contraseña - LinkUp 🔑",
            EmailTemplateService.PasswordReset(user.FirstName, resetLink));

        return R.Success();
    }

    public async Task<R> ResetPasswordAsync(string token, string newPassword)
    {
        var user = _userManager.Users.FirstOrDefault(u =>
            u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow);
        if (user == null)
            return R.Failure("Token inválido o expirado.");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return R.Failure("Error al actualizar la contraseña.");

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
            return R.Failure(string.Join(", ", addResult.Errors.Select(e => e.Description)));

        user.IsActive = true;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return R.Success();
    }

    public async Task<R> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return R.Failure("Usuario no encontrado.");

        if (dto.ProfilePictureData != null && dto.ProfilePictureData.Length > 0
            && !string.IsNullOrEmpty(dto.ProfilePictureFileName))
        {
            user.ProfilePicture = await _fileStorage.SaveFileAsync(
                dto.ProfilePictureData, dto.ProfilePictureFileName, "profiles");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Phone = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            await _userManager.RemovePasswordAsync(user);
            var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
            if (!addResult.Succeeded)
                return R.Failure(string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }

        await _userManager.UpdateAsync(user);
        return R.Success();
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user == null ? null : _mapper.Map<UserResponseDto>(user);
    }
}
