using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;
using ViSah.Interfaces;

namespace ViSah.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController(AppDbContext db, ICoverManagementService coverService) : ControllerBase
{
    [HttpGet]
    public Task<List<Album>> Get() =>
        db.Albums.AsNoTracking().Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var album = await db.Albums.AsNoTracking().Include(a => a.Artist).Include(a => a.Tracks)
            .SingleOrDefaultAsync(a => a.Id == id);
        
        if (album is null)
        {
            return NotFound();
        }

        return Ok(album);
    }

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> Create(Album album)
    {
        db.Albums.Add(album);
        
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = album.Id }, album);
    }

    [Authorize(Roles = "Admin"), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Album updatedAlbum)
    {
        var album = await db.Albums.FindAsync(id);
        
        if (album is null)
        {
            return NotFound();
        }

        album.Title = updatedAlbum.Title;
        album.ArtistId = updatedAlbum.ArtistId;
        album.ReleaseDate = updatedAlbum.ReleaseDate;
        album.CoverUrl = updatedAlbum.CoverUrl;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(int id, IFormFile image, CancellationToken cancellationToken)
    {
        try
        {
            var uploadedImage = await coverService.UploadAlbumCoverAsync(id, image, cancellationToken);
            if (uploadedImage is null)
            {
                return NotFound(new { message = "Álbum não encontrado." });
            }

            return Ok(new { albumId = id, uploadedImage.Url });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var album = await db.Albums.FindAsync(id);
        if (album is null)
        {
            return NotFound();
        }

        db.Albums.Remove(album);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
