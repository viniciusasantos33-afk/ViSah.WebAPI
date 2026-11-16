using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;
using System.Security.Claims;

namespace ViSah.Controllers;

[ApiController]
[Authorize]
[Route("api/favorites")]
public class FavoritesController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await db.Favorites.AsNoTracking().Where(x => x.UserId == UserId)
        .Include(x => x.Track).Include(x => x.Artist).ToListAsync());

    [HttpPost("tracks/{id:int}")]
    public async Task<IActionResult> AddTrack(int id)
    {
        if (!await db.Tracks.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        if (!await db.Favorites.AnyAsync(x => x.UserId == UserId && x.TrackId == id))
        {
            db.Favorites.Add(new Favorite { UserId = UserId, TrackId = id });
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("tracks/{id:int}")]
    public async Task<IActionResult> DelTrack(int id)
    {
        var x = await db.Favorites.SingleOrDefaultAsync(x => x.UserId == UserId && x.TrackId == id);
        if (x is null)
        {
            return NotFound();
        }

        db.Favorites.Remove(x);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("artists/{id:int}")]
    public async Task<IActionResult> AddArtist(int id)
    {
        if (!await db.Artists.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        if (!await db.Favorites.AnyAsync(x => x.UserId == UserId && x.ArtistId == id))
        {
            db.Favorites.Add(new Favorite { UserId = UserId, ArtistId = id });
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("artists/{id:int}")]
    public async Task<IActionResult> DelArtist(int id)
    {
        var x = await db.Favorites.SingleOrDefaultAsync(x => x.UserId == UserId && x.ArtistId == id);
        if (x is null)
        {
            return NotFound();
        }

        db.Favorites.Remove(x);
        await db.SaveChangesAsync();
        return NoContent();
    }
}