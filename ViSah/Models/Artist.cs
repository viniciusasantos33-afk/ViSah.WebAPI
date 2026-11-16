using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Artist
{
    public int Id { get; set; }
    [MaxLength(120)] public required string Name { get; set; }
    [MaxLength(2000)] public string? Biography { get; set; }
    [MaxLength(500)] public string? PhotoUrl { get; set; }
    public long PlayCount { get; set; }
    public ICollection<Album> Albums { get; set; } = [];
}