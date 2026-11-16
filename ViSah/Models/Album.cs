using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Album
{
    public int Id { get; set; }
    [MaxLength(160)] public required string Title { get; set; }
    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    [MaxLength(500)] public string? CoverUrl { get; set; }
    [MaxLength(255)] public string? CoverPublicId { get; set; }
    public ICollection<Track> Tracks { get; set; } = [];
}
