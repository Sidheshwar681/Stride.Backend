using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stride.Api.Services;
using Stride.Api.Storage;

namespace Stride.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly PasswordHasher _passwordHasher;
    private readonly TokenService _tokens;

    public AuthController(UserRepository users, PasswordHasher passwordHasher, TokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public sealed class RegisterRequest
    {
        [Required, MinLength(2), MaxLength(40)]
        public string Username { get; init; } = "";

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; init; } = "";

        [Required, MinLength(6), MaxLength(200)]
        public string Password { get; init; } = "";
    }

    public sealed class LoginRequest
    {
        [Required, MinLength(2), MaxLength(200)]
        public string Identifier { get; init; } = "";

        [Required, MinLength(1), MaxLength(200)]
        public string Password { get; init; } = "";
    }

    public sealed record AuthResponse(Guid UserId, string Username, string Email, string AccessToken);

    public sealed record MeResponse(Guid UserId, string Username, string Email, DateTimeOffset CreatedAt);

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var passwordHash = _passwordHasher.Hash(request.Password);
        var (user, error) = await _users.CreateAsync(request.Username, request.Email, passwordHash, cancellationToken);
        if (user is null)
        {
            return BadRequest(new { message = error ?? "Unable to register." });
        }

        var token = _tokens.CreateAccessToken(user);
        return Ok(new AuthResponse(user.Id, user.Username, user.Email, token));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.FindByEmailOrUsernameAsync(request.Identifier, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = _tokens.CreateAccessToken(user);
        return Ok(new AuthResponse(user.Id, user.Username, user.Email, token));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(user.Id, user.Username, user.Email, user.CreatedAt));
    }
}
