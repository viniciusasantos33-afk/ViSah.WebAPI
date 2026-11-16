using Microsoft.AspNetCore.Http;
using ViSah.Services.Media;

namespace ViSah.Interfaces;

public interface ICloudinaryAudioService
{
    Task<UploadedAudio> UploadAsync(IFormFile audio, CancellationToken cancellationToken);
    Task DeleteAsync(string publicId);
}
