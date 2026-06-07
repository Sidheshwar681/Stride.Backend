using System.ComponentModel.DataAnnotations;

namespace Stride.Api.DTOs;

public sealed class LoginRequest
{
    [Required, MinLength(2), MaxLength(200)]
    public string Identifier { get; set; } = "";

    [Required, MinLength(1), MaxLength(200)]
    public string Password { get; set; } = "";
}