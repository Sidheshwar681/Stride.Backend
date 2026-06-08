using System.ComponentModel.DataAnnotations;

namespace Stride.Api.DTOs;
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}