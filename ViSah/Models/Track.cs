using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Track
{
    public int Id { get; set; }
    [MaxLength(160)] public required string Title { get; set; }
    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }
    public int? AlbumId { get; set; }
    public Album? Album { get; set; }
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }
    public int MoodId { get; set; }
    public Mood? Mood { get; set; }
    public int DurationSeconds { get; set; }
    [MaxLength(1000)] public string? AudioUrl { get; set; }
    [MaxLength(255)] public string? AudioPublicId { get; set; }
    [MaxLength(500)] public string? CoverUrl { get; set; }
    [MaxLength(255)] public string? CoverPublicId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long PlayCount { get; set; }
}
