using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Models;
using ViSah.Interfaces;

namespace ViSah.Services;

public sealed class CatalogService(AppDbContext db) : IMusicService
{
    public async Task<object> Search(TrackQuery q)
    {
        var items = db.Tracks.AsNoTracking().Include(x => x.Artist).Include(x => x.Album).Include(x => x.Genre)
            .Include(x => x.Mood).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.Search))
            items = items.Where(x => x.Title.Contains(q.Search) || x.Artist!.Name.Contains(q.Search));
        if (q.GenreId is not null) items = items.Where(x => x.GenreId == q.GenreId);
        if (q.MoodId is not null) items = items.Where(x => x.MoodId == q.MoodId);
        var total = await items.CountAsync();
        var page = Math.Max(1, q.Page);
        var size = Math.Clamp(q.PageSize, 1, 100);
        return new
        {
            total,
            page,
            pageSize = size,
            items = await items.OrderByDescending(x => x.PlayCount).Skip((page - 1) * size).Take(size).ToListAsync()
        };
    }

    public async Task<Track> CreateAsync(TrackInput request, CancellationToken cancellationToken)
    {
        var track = new Track
        {
            Title = request.Title.Trim(),
            ArtistId = request.ArtistId,
            AlbumId = request.AlbumId,
            GenreId = request.GenreId,
            MoodId = request.MoodId,
            DurationSeconds = request.DurationSeconds,
            AudioUrl = request.AudioUrl?.Trim(),
            CoverUrl = request.CoverUrl
        };

        db.Tracks.Add(track);
        await db.SaveChangesAsync(cancellationToken);
        return track;
    }

    public async Task<bool> UpdateAsync(int id, TrackInput request, CancellationToken cancellationToken)
    {
        var track = await db.Tracks.FindAsync([id], cancellationToken);
        if (track is null)
        {
            return false;
        }

        track.Title = request.Title.Trim();
        track.ArtistId = request.ArtistId;
        track.AlbumId = request.AlbumId;
        track.GenreId = request.GenreId;
        track.MoodId = request.MoodId;
        track.DurationSeconds = request.DurationSeconds;
        track.AudioUrl = request.AudioUrl?.Trim();
        track.CoverUrl = request.CoverUrl;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var track = await db.Tracks.FindAsync([id], cancellationToken);
        if (track is null)
        {
            return false;
        }

        db.Tracks.Remove(track);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RecordPlayAsync(int id, CancellationToken cancellationToken)
    {
        var track = await db.Tracks.FindAsync([id], cancellationToken);
        if (track is null)
        {
            return false;
        }

        track.PlayCount++;
        var artist = await db.Artists.FindAsync([track.ArtistId], cancellationToken);
        if (artist is not null)
        {
            artist.PlayCount++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Track?> GetTrack(int id) => db.Tracks.AsNoTracking().Include(x => x.Artist).Include(x => x.Album)
        .Include(x => x.Genre).Include(x => x.Mood).SingleOrDefaultAsync(x => x.Id == id);

    public async Task<object> Recommend(int moodId, int? genreId, int userId)
    {
        var exact = db.Tracks.AsNoTracking().Where(x => x.MoodId == moodId);
        if (genreId.HasValue) exact = exact.Where(x => x.GenreId == genreId);
        var tracks = await exact.Include(x => x.Artist).Include(x => x.Genre).Include(x => x.Mood)
            .OrderByDescending(x => x.PlayCount).Take(12).ToListAsync();
        var fallback = tracks.Count == 0;
        if (fallback)
        {
            var favoriteMoods = await db.Favorites.Where(f => f.UserId == userId && f.TrackId != null)
                .Select(f => f.Track!.MoodId).ToListAsync();
            var preferred = favoriteMoods.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key)
                .FirstOrDefault();
            tracks = await db.Tracks.AsNoTracking().Where(x => x.MoodId == preferred).Include(x => x.Artist)
                .Include(x => x.Genre).Include(x => x.Mood).OrderByDescending(x => x.PlayCount).Take(12).ToListAsync();
        }

        return new { fallback, items = tracks };
    }
}
