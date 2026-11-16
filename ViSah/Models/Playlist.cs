using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Playlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    [MaxLength(120)] public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PlaylistTrack> Tracks { get; set; } = [];
}