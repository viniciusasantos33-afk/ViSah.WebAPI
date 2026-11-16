namespace ViSah.Models;

public record PlaylistInput(string Name);

public record PlaylistTrackInput(int TrackId);

public record PlaylistOrder(List<int> TrackIds);