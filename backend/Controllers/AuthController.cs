using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stride.Api.Services;
using Stride.Api.Storage;
using Stride.Api.Models;
using Stride.Api.DTOs;

namespace Stride.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly PasswordHasher _passwordHasher;
    private readonly TokenService _tokens;

    public AuthController(
        UserRepository users,
        PasswordHasher passwordHasher,
        TokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    // ================= REGISTER =================

    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var (user, error) = await _users.CreateAsync(
            request.Username,
            request.Email,
            request.Password,
            cancellationToken);

        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new
        {
            user!.Id,
            user.Username,
            user.Email,
            user.CreatedAt
        });
    }

    // ================= LOGIN =================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByEmailOrUsernameAsync(
            request.Identifier,
            cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = _tokens.CreateAccessToken(user);

        return Ok(new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            token));
    }

    // ================= ME =================

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt));
    }

    [AllowAnonymous]
    [HttpGet("user/{id:guid}")]
    public async Task<ActionResult<MeResponse>> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new MeResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt));
    }

    [AllowAnonymous]
    [HttpGet("user")]
    public async Task<ActionResult<MeResponse>> GetUserByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email query is required." });
        }

        var user = await _users.FindByEmailOrUsernameAsync(email, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new MeResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt));
    }

    // ================= VERSION =================

    [AllowAnonymous]
    [HttpGet("version")]
    public IActionResult Version()
    {
        return Ok("Build: 2026-05-30-v2");
    }
}