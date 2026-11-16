using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Interfaces;

namespace ViSah.Services;

public sealed class AudioDeliveryService(AppDbContext db) : IAudioDeliveryService
{
    public async Task<string?> GetAudioUrlAsync(int trackId, CancellationToken cancellationToken)
    {
        var track = await db.Tracks
            .AsNoTracking()
            .Where(track => track.Id == trackId)
            .Select(track => new { track.AudioUrl })
            .SingleOrDefaultAsync(cancellationToken);

        if (track?.AudioUrl is null || !Uri.TryCreate(track.AudioUrl, UriKind.Absolute, out var audioUri))
        {
            return null;
        }

        if (audioUri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return audioUri.ToString();
    }
}
