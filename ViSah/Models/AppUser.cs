using System.ComponentModel.DataAnnotations;

namespace ViSah.Models;

public sealed class AppUser
{
    public int Id { get; set; }
    [MaxLength(120)] public required string Name { get; set; }
    [MaxLength(256)] public required string Email { get; set; }
    [MaxLength(100)] public required string PasswordHash { get; set; }
    [MaxLength(500)] public string? PhotoUrl { get; set; }
    [MaxLength(20)] public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}