using System.ComponentModel.DataAnnotations;

namespace Stride.Api.DTOs;

public sealed class RegisterRequest
{
    [Required, MinLength(2), MaxLength(40)]
    public string Username { get; set; } = "";

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required, MinLength(6), MaxLength(200)]
    public string Password { get; set; } = "";
}