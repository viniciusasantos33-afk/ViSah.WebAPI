using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ViSah.Data;
using ViSah.Models;
using ViSah.Interfaces;

namespace ViSah.Services;

public sealed class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<string?> Register(RegisterRequest r)
    {
        if (await db.Users.AnyAsync(x => x.Email == r.Email.Trim().ToLower())) return null;
        var u = new AppUser
        {
            Name = r.Name.Trim(),
            Email = r.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password)
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return Token(u);
    }

    public async Task<(string?, string?)> Login(LoginRequest r)
    {
        var u = await db.Users.SingleOrDefaultAsync(x => x.Email == r.Email.Trim().ToLower());
        if (u is null || !u.IsActive || !BCrypt.Net.BCrypt.Verify(r.Password, u.PasswordHash))
            return (null, "Credenciais inválidas ou conta desativada.");
        return (Token(u), null);
    }

    string Token(AppUser u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()), new Claim(ClaimTypes.Name, u.Name),
            new Claim(ClaimTypes.Email, u.Email), new Claim(ClaimTypes.Role, u.Role)
        };
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(config["Jwt:Issuer"] ?? "ViSah",
            config["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(config.GetValue<int>("Jwt:TokenLifetimeHours")),
            signingCredentials: creds));
    }
}