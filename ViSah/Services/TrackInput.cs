namespace ViSah.Services;

public record TrackInput(
    string Title,
    int ArtistId,
    int? AlbumId,
    int GenreId,
    int MoodId,
    int DurationSeconds,
    string? AudioUrl,
    string? CoverUrl);
