using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;

namespace ViSah.Controllers;

[ApiController]
[Route("api/moods")]
public class MoodsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<Mood>> Get() => db.Moods.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> Create(Mood x)
    {
        db.Moods.Add(x);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), x);
    }

    [Authorize(Roles = "Admin"), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Mood v)
    {
        var x = await db.Moods.FindAsync(id);
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
        var x = await db.Moods.FindAsync(id);
        if (x is null)
        {
            return NotFound();
        }

        db.Moods.Remove(x);
        await db.SaveChangesAsync();
        return NoContent();
    }
}