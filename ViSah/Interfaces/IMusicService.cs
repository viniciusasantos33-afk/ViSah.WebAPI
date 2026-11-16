using ViSah.Models;
using ViSah.Services;

namespace ViSah.Interfaces;

public interface IMusicService : ICatalogService
{
    Task<Track> CreateAsync(TrackInput request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, TrackInput request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> RecordPlayAsync(int id, CancellationToken cancellationToken);
}
