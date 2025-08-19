using System.ComponentModel.DataAnnotations;

namespace project.Models.User;

public class UserSignupFormModel
{
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Your Name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email format")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain uppercase and lowercase letters, and numbers.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please repeat the password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    [Display(Name = "Repeat password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // [CORRECTED] Using 'Range' for checkbox validation.
    // 'Required' does not work as 'false' is a valid value.
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the site's terms")]
    [Display(Name = "I agree to the terms")]
    public bool Agree { get; set; }
}