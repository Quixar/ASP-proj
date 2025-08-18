using System.ComponentModel.DataAnnotations;

namespace project.Models.User;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string PasswordHash { get; set; }
}