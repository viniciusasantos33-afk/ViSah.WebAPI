using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int? TrackId { get; set; }
    public Track? Track { get; set; }
    public int? ArtistId { get; set; }
    public Artist? Artist { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}