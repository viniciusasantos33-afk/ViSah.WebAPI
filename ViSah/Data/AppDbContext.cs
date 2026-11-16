using Microsoft.EntityFrameworkCore;
using ViSah.Models;

namespace ViSah.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Mood> Moods => Set<Mood>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Genre>().HasIndex(x => x.Name).IsUnique();
        b.Entity<Mood>().HasIndex(x => x.Name).IsUnique();
        b.Entity<Favorite>().HasIndex(x => new { x.UserId, x.TrackId });
        b.Entity<Favorite>().HasIndex(x => new { x.UserId, x.ArtistId });
        b.Entity<PlaylistTrack>().HasKey(x => new { x.PlaylistId, x.TrackId });
        b.Entity<PlaylistTrack>().HasIndex(x => new { x.PlaylistId, x.Position }).IsUnique();
        b.Entity<Album>().HasOne(x => x.Artist).WithMany(x => x.Albums).HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Track>().HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Track>().HasOne(x => x.Album).WithMany(x => x.Tracks).HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);
        b.Entity<Track>().HasOne(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Track>().HasOne(x => x.Mood).WithMany().HasForeignKey(x => x.MoodId).OnDelete(DeleteBehavior.Restrict);
    }
}