using System.ComponentModel.DataAnnotations;

namespace LolBuilds.Api.Dtos;

public class RegisterRequest
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and _.")]
    public string Username { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = "";
}

public record UserDto(int Id, string Username, string Email, string Role);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserDto User);
