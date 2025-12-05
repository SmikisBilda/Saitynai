using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Saitynai.DTOs;
using System.Threading.Tasks;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PostgresContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(PostgresContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (await _context.User.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
        {
            return BadRequest("Username or email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash
        };

        _context.User.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = user.Id }, "User registered successfully");
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userIdString == null || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("Invalid token");
        }

        var user = await _context.User
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new 
        { 
            id = user.Id,
            username = user.Username,
            email = user.Email,
            roles = user.UserRoles.Select(ur => new { id = ur.Role.Id, name = ur.Role.Name }).ToList()
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await _context.User
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        var accessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken.Token,
            ExpiresOn = newRefreshToken.ExpiresOn,
            CreatedOn = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { AccessToken = accessToken, RefreshToken = newRefreshToken.Token });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var providedRefreshToken = request.RefreshToken;
        var storedToken = await _context.RefreshTokens
                                        .Include(t => t.User)
                                            .ThenInclude(u => u.UserRoles)
                                            .ThenInclude(ur => ur.Role)
                                        .SingleOrDefaultAsync(t => t.Token == providedRefreshToken);

        if (storedToken == null)
        {
            return Unauthorized("Invalid token.");
        }

        if (storedToken.IsRevoked)
        {
            await RevokeTokenFamily(storedToken);
            await _context.SaveChangesAsync(); 
            return Unauthorized("Invalid token.");
        }

        if (!storedToken.IsActive)
        {
            return Unauthorized("Invalid token.");
        }

        var newAccessToken = GenerateJwtToken(storedToken.User);
        var newRefreshToken = GenerateRefreshToken();

        storedToken.RevokedOn = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken.Token,
            ExpiresOn = newRefreshToken.ExpiresOn,
            CreatedOn = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Invalid token claims.");
        }

        var storedToken = await _context.RefreshTokens
                                        .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Ok("Successfully logged out.");
        }

        if (storedToken.UserId != userId)
        {
            return Forbid("You do not have permission to revoke this token.");
        }

        if (storedToken.IsActive)
        {
            storedToken.RevokedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok("Successfully logged out.");
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    
    public record GeneratedRefreshToken(string Token, DateTime ExpiresOn);
    
    private GeneratedRefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        var refreshToken = new GeneratedRefreshToken(
            Token: Convert.ToBase64String(randomNumber),
            ExpiresOn: DateTime.UtcNow.AddDays(7)
        );

        return refreshToken;
    }

    private async Task RevokeTokenFamily(RefreshToken token)
    {
        if (!string.IsNullOrEmpty(token.ReplacedByToken))
        {
            var childToken = await _context.RefreshTokens
                                        .SingleOrDefaultAsync(t => t.Token == token.ReplacedByToken);
            if (childToken != null && childToken.IsActive)
            {
                await RevokeTokenFamily(childToken);
            }
        }
        token.RevokedOn = DateTime.UtcNow;
    }
}
