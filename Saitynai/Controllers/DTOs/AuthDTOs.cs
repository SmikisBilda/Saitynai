using System.ComponentModel.DataAnnotations;

namespace Saitynai.DTOs;

public class RegisterDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class LoginDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}

public class RefreshTokenRequestDto
{
    [Required]
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}

public class LogoutRequestDto
{
    [Required]
    public string RefreshToken { get; set; }
}
