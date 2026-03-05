using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Request;
using LinkUp.Domain.Entities;
using LinkUp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileStorageService _fileStorage;

    public AccountController(IAccountService accountService,
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IFileStorageService fileStorage)
    {
        _accountService = accountService;
        _signInManager = signInManager;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? message = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Posts");

        if (!string.IsNullOrEmpty(message))
            ViewBag.Message = message;

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _accountService.LoginAsync(new LoginRequestDto
        {
            UserName = model.UserName,
            Password = model.Password
        });

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName);
        await _signInManager.SignInAsync(user!, isPersistent: false);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Posts");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Posts");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Save profile picture in Web layer, pass path to Application
        string? picturePath = null;
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.ProfilePicture.CopyToAsync(ms);
            picturePath = await _fileStorage.SaveFileAsync(
                ms.ToArray(), model.ProfilePicture.FileName, "profiles");
        }

        var activationUrl = Url.Action("ActivateAccount", "Account", null, Request.Scheme)!;

        var result = await _accountService.RegisterAsync(new RegisterRequestDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            Email = model.Email,
            UserName = model.UserName,
            Password = model.Password,
            ProfilePicturePath = picturePath
        }, activationUrl);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        TempData["Success"] = "Tu cuenta fue creada. Revisa tu correo para activarla.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ActivateAccount(string token)
    {
        var result = await _accountService.ActivateAccountAsync(token);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Cuenta activada. Ahora puedes iniciar sesión."
            : result.Error;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var resetUrl = Url.Action("ResetPassword", "Account", null, Request.Scheme)!;
        var result = await _accountService.ForgotPasswordAsync(model.UserName, resetUrl);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        TempData["Success"] = "Se envió un enlace a tu correo para restablecer la contraseña.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) =>
        View(new ResetPasswordViewModel { Token = token });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _accountService.ResetPasswordAsync(model.Token, model.Password);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        TempData["Success"] = "Contraseña restablecida. Ya puedes iniciar sesión.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var user = await _accountService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        return View(new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            CurrentProfilePicture = user.ProfilePicture
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden.");
            return View(model);
        }

        byte[]? picData = null;
        string? picName = null;
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.ProfilePicture.CopyToAsync(ms);
            picData = ms.ToArray();
            picName = model.ProfilePicture.FileName;
        }

        var userId = int.Parse(_userManager.GetUserId(User)!);
        var result = await _accountService.UpdateProfileAsync(userId, new UpdateProfileRequestDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            NewPassword = model.Password,
            ProfilePictureData = picData,
            ProfilePictureFileName = picName
        });

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        TempData["Success"] = "Perfil actualizado.";
        return RedirectToAction("Index", "Posts");
    }

    public IActionResult AccessDenied() => View();
}
