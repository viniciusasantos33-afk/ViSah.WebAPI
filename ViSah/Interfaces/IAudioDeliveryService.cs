namespace ViSah.Interfaces;

public interface IAudioDeliveryService
{
    Task<string?> GetAudioUrlAsync(int trackId, CancellationToken cancellationToken);
}