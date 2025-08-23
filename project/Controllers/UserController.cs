using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Data.Entities;
using project.Models.User;
using project.Services.Kdf;
using System.Security.Claims;
using System.Security.Cryptography;

namespace project.Controllers;

public class UserController : Controller
{
    private readonly DataContext _context;
    private readonly IKdfService _kdfService;

    public UserController(DataContext context, IKdfService kdfService)
    {
        _context = context;
        _kdfService = kdfService;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new UserSignInFormModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(UserSignInFormModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var userAccess = await _context.UserAccesses
            .Include(x => x.UserData)
            .Include(x => x.UserRole)
            .FirstOrDefaultAsync(x => x.Login == model.Username);

        if (userAccess == null || !VerifyPassword(model.Password, userAccess.Salt, userAccess.Dk))
        {
            // Запоминаем ошибку в TempData для отображения в Layout
            TempData["AuthError"] = "Invalid username or password";
            return View(model);
        }

        await SignInAsync(
            userAccess.UserId.ToString(),
            userAccess.UserData.Name,
            userAccess.UserRole.Id,
            model.RememberMe
        );

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult SignUp()
    {
        return View(new UserSignupFormModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(UserSignupFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _context.UserAccesses.AnyAsync(x => x.Login == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Username already taken");
            return View(model);
        }
        if (await _context.Users.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already registered");
            return View(model);
        }

        var userData = new UserData
        {
            Name = model.Name,
            Email = model.Email
        };
        _context.Users.Add(userData);
        await _context.SaveChangesAsync();

        var userRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.Id == "User");
        if (userRole == null)
        {
            userRole = new UserRole
            {
                Id = "User",
                Description = "Default user role",
                CanCreate = false,
                CanRead = true,
                CanUpdate = false,
                CanDelete = false
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        var salt = GenerateSalt();
        var dk = _kdfService.Dk(model.Password, salt);

        var userAccess = new UserAccess
        {
            Login = model.Username,
            Salt = salt,
            Dk = dk,
            UserId = userData.Id,
            RoleId = userRole.Id.ToString()
        };

        _context.UserAccesses.Add(userAccess);
        await _context.SaveChangesAsync();

        await SignInAsync(userData.Id.ToString(), userData.Name, userRole.Id);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(SignIn));
    }

    private async Task SignInAsync(string userId, string userName, string role, bool isPersistent = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            AllowRefresh = true,
            ExpiresUtc = isPersistent
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
    }

    private string GenerateSalt()
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(salt);
    }

    private bool VerifyPassword(string password, string salt, string storedHash)
    {
        var hash = _kdfService.Dk(password, salt);
        return hash == storedHash;
    }
}
