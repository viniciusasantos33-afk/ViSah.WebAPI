using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;

namespace ViSah.Controllers;

[ApiController, Authorize(Roles = "Admin"), Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<UserListItem>> Get() => db.Users.AsNoTracking()
        .Select(x => new UserListItem(x.Id, x.Name, x.Email, x.Role, x.IsActive)).ToListAsync();

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserAdminInput request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.Name = request.Name.Trim();
        if (request.Role == "Admin")
        {
            user.Role = "Admin";
        }
        else
        {
            user.Role = "User";
        }

        user.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record UserListItem(int Id, string Name, string Email, string Role, bool IsActive);

public record UserAdminInput(string Name, string Role, bool IsActive);