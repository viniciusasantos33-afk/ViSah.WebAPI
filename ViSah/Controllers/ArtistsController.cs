using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;

namespace ViSah.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<Artist>> Get() => db.Artists.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var artist = await db.Artists.AsNoTracking().Include(a => a.Albums)
            .SingleOrDefaultAsync(a => a.Id == id);
        if (artist is null)
        {
            return NotFound();
        }

        return Ok(artist);
    }

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> Create(Artist x)
    {
        db.Artists.Add(x);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = x.Id }, x);
    }

    [Authorize(Roles = "Admin"), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Artist v)
    {
        var x = await db.Artists.FindAsync(id);
        if (x is null)
        {
            return NotFound();
        }

        x.Name = v.Name;
        x.Biography = v.Biography;
        x.PhotoUrl = v.PhotoUrl;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await db.Artists.FindAsync(id);
        if (x is null)
        {
            return NotFound();
        }

        db.Artists.Remove(x);
        await db.SaveChangesAsync();
        return NoContent();
    }
}