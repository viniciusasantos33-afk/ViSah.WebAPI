using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;

namespace ViSah.Controllers;

[ApiController, Authorize, Route("api/profile")]
public class ProfileController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await db.Users.AsNoTracking().Where(x => x.Id == UserId)
            .Select(x => new { x.Id, x.Name, x.Email, x.PhotoUrl }).SingleOrDefaultAsync();
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProfileInput request)
    {
        var user = await db.Users.FindAsync(UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "O nome é obrigatório." });
        }

        user.Name = request.Name.Trim();
        user.PhotoUrl = request.PhotoUrl;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 8)
                return BadRequest(new { message = "A senha deve conter ao menos 8 caracteres." });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record ProfileInput(string Name, string? PhotoUrl, string? Password);