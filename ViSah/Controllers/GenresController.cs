using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;

namespace ViSah.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<Genre>> Get() => db.Genres.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> Create(Genre x)
    {
        db.Genres.Add(x);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), x);
    }

    [Authorize(Roles = "Admin"), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Genre v)
    {
        var x = await db.Genres.FindAsync(id);
        if (x is null)
        {
            return NotFound();
        }

        x.Name = v.Name;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await db.Genres.FindAsync(id);
        if (x is null)
        {
            return NotFound();
        }

        db.Genres.Remove(x);
        await db.SaveChangesAsync();
        return NoContent();
    }
}