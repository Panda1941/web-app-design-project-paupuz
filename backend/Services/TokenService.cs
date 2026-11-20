using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using LolBuilds.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace LolBuilds.Api.Services;

// Creates access tokens (JWT) and refresh tokens (random strings).
public class TokenService
{
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    // Access token: a signed JWT that says who the user is (id, username, role) and when it expires.
    // Signed with HMAC-SHA256 using the secret key from configuration (Jwt:Key).
    public string CreateAccessToken(User user)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),          // user ID
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("role", user.Role),                                       // User or Admin
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // unique token ID
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey(_config)));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(AccessTokenLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Refresh token: a long random string. Not a JWT; it only works together with the database row.
    public static string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    // Only the hash of a refresh token is stored in the database.
    public static string Hash(string refreshToken)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }

    public static string GetSigningKey(IConfiguration config)
    {
        string? key = config["Jwt:Key"];
        if (string.IsNullOrEmpty(key) || key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters long.");
        return key;
    }
}
