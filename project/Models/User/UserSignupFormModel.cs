using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace project.Models.User;

public class UserSignupFormModel
{
    [Required, FromForm(Name = "username")]
    public string Username { get; set; } = string.Empty;

    [FromForm(Name = "name")]
    public string? Name { get; set; }

    [Required, EmailAddress, FromForm(Name = "email")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), FromForm(Name = "password")]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), FromForm(Name = "repeat-password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}