using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Interfaces;
using ViSah.Services.Media;

namespace ViSah.Services;

public sealed class AudioManagementService(
    AppDbContext db,
    ICloudinaryAudioService audioService,
    ILogger<AudioManagementService> logger) : IAudioManagementService
{
    public async Task<UploadedAudio?> UploadTrackAudioAsync(
        int trackId,
        IFormFile audio,
        CancellationToken cancellationToken)
    {
        var track = await db.Tracks.SingleOrDefaultAsync(item => item.Id == trackId, cancellationToken);
        if (track is null)
        {
            return null;
        }

        var uploadedAudio = await audioService.UploadAsync(audio, cancellationToken);
        var previousPublicId = track.AudioPublicId;
        track.AudioUrl = uploadedAudio.Url;
        track.AudioPublicId = uploadedAudio.PublicId;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TryDeleteAudioAsync(uploadedAudio.PublicId);
            throw;
        }

        await TryDeletePreviousAudioAsync(previousPublicId);
        return uploadedAudio;
    }

    private async Task TryDeletePreviousAudioAsync(string? publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        await TryDeleteAudioAsync(publicId);
    }

    private async Task TryDeleteAudioAsync(string publicId)
    {
        try
        {
            await audioService.DeleteAsync(publicId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Falha ao remover áudio {PublicId} do Cloudinary.", publicId);
        }
    }
}
