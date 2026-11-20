using LolBuilds.Api.Data;
using LolBuilds.Api.Dtos;
using LolBuilds.Api.Models;
using LolBuilds.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolBuilds.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    /// <summary>Register a new user (role User) and log them in.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        string username = request.Username.Trim();
        string email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Username == username))
            return ConflictProblem("This username is already taken.");
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return ConflictProblem("This email is already registered.");

        var user = new User
        {
            Username = username,
            Email = email,
            Role = Roles.User, // new accounts are never admins
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        AuthResponse response = await IssueTokens(user);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Log in with username and password. Returns an access token and a refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username.Trim());
        if (user == null)
            return UnauthorizedProblem("Wrong username or password.");

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return UnauthorizedProblem("Wrong username or password.");

        return Ok(await IssueTokens(user));
    }

    /// <summary>Exchange a refresh token for a new access token and a new refresh token (the old one stops working).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        string hash = TokenService.Hash(request.RefreshToken);
        RefreshToken? stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            return UnauthorizedProblem("The refresh token is invalid or expired. Please log in again.");

        // A token that was already used is being used again: someone may have stolen it.
        // To be safe, log the user out everywhere.
        if (stored.RevokedAt != null)
        {
            await RevokeAllTokens(stored.UserId);
            return UnauthorizedProblem("The refresh token was already used. Please log in again.");
        }

        // Rotation: the used token is revoked and a brand new pair is issued.
        stored.RevokedAt = DateTime.UtcNow;
        return Ok(await IssueTokens(stored.User!));
    }

    /// <summary>Log out: revokes all of the current user's refresh tokens.</summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await RevokeAllTokens(CurrentUserId);
        return NoContent();
    }

    /// <summary>Get the currently logged-in user.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me()
    {
        User? user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null)
            return UnauthorizedProblem("User no longer exists.");
        return Ok(ToDto(user));
    }

    private async Task<AuthResponse> IssueTokens(User user)
    {
        string refreshToken = TokenService.CreateRefreshToken();
        var stored = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenService.Hash(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(TokenService.RefreshTokenLifetime),
        };
        _db.RefreshTokens.Add(stored);
        await _db.SaveChangesAsync();

        return new AuthResponse(
            _tokens.CreateAccessToken(user),
            DateTime.UtcNow.Add(TokenService.AccessTokenLifetime),
            refreshToken,
            stored.ExpiresAt,
            ToDto(user));
    }

    private async Task RevokeAllTokens(int userId)
    {
        List<RefreshToken> active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();
        foreach (RefreshToken token in active)
            token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(user.Id, user.Username, user.Email, user.Role);
    }
}
