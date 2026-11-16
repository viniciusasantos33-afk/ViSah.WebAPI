using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class Genre
{
    public int Id { get; set; }
    [MaxLength(80)] public required string Name { get; set; }
}