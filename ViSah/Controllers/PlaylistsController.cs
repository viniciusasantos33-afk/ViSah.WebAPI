using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;

namespace ViSah.Controllers;

[ApiController, Authorize, Route("api/playlists")]
public class PlaylistsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public Task<List<Playlist>> Get() => db.Playlists.AsNoTracking()
        .Where(x => x.UserId == UserId).Include(x => x.Tracks).ThenInclude(x => x.Track)
        .OrderBy(x => x.Name).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create(PlaylistInput request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest();
        }

        var playlist = new Playlist { Name = request.Name.Trim(), UserId = UserId };
        db.Playlists.Add(playlist);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Rename(int id, PlaylistInput request)
    {
        var playlist = await db.Playlists.SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
        if (playlist is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest();
        }

        playlist.Name = request.Name.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var playlist = await db.Playlists.SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
        if (playlist is null)
        {
            return NotFound();
        }

        db.Playlists.Remove(playlist);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/tracks")]
    public async Task<IActionResult> AddTrack(int id, PlaylistTrackInput request)
    {
        if (!await db.Playlists.AnyAsync(x => x.Id == id && x.UserId == UserId))
        {
            return NotFound();
        }

        if (!await db.Tracks.AnyAsync(x => x.Id == request.TrackId))
        {
            return NotFound();
        }

        if (!await db.PlaylistTracks.AnyAsync(x => x.PlaylistId == id && x.TrackId == request.TrackId))
        {
            var position = await db.PlaylistTracks.CountAsync(x => x.PlaylistId == id);
            db.PlaylistTracks.Add(new PlaylistTrack
                { PlaylistId = id, TrackId = request.TrackId, Position = position });
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPut("{id:int}/tracks/order")]
    public async Task<IActionResult> Reorder(int id, PlaylistOrder request)
    {
        if (!await db.Playlists.AnyAsync(x => x.Id == id && x.UserId == UserId))
        {
            return NotFound();
        }

        var entries = await db.PlaylistTracks.Where(x => x.PlaylistId == id).ToListAsync();
        if (entries.Count != request.TrackIds.Distinct().Count() ||
            !entries.Select(x => x.TrackId).ToHashSet().SetEquals(request.TrackIds)) return BadRequest();

        foreach (var entry in entries) entry.Position = -entry.Position - 1;
        await db.SaveChangesAsync();
        for (var i = 0; i < request.TrackIds.Count; i++)
            entries.Single(x => x.TrackId == request.TrackIds[i]).Position = i;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}/tracks/{trackId:int}")]
    public async Task<IActionResult> RemoveTrack(int id, int trackId)
    {
        var entry = await db.PlaylistTracks.Include(x => x.Playlist)
            .SingleOrDefaultAsync(x => x.PlaylistId == id && x.TrackId == trackId && x.Playlist!.UserId == UserId);
        if (entry is null)
        {
            return NotFound();
        }

        db.PlaylistTracks.Remove(entry);
        await db.SaveChangesAsync();
        return NoContent();
    }
}