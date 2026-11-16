using Microsoft.AspNetCore.Http;
using ViSah.Services.Media;

namespace ViSah.Interfaces;

public interface IAudioManagementService
{
    Task<UploadedAudio?> UploadTrackAudioAsync(
        int trackId,
        IFormFile audio,
        CancellationToken cancellationToken);
}
