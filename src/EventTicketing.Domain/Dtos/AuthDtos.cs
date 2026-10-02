using System.ComponentModel.DataAnnotations;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = null!;

    [Phone]
    [MaxLength(32)]
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public AuthResponseDto(
        string token,
        DateTime expirationUtc,
        int userId,
        string email,
        string fullName,
        UserRole role)
    {
        Token = token;
        ExpirationUtc = expirationUtc;
        UserId = userId;
        Email = email;
        FullName = fullName;
        Role = role;
    }

    public string Token { get; set; }
    public DateTime ExpirationUtc { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
}
