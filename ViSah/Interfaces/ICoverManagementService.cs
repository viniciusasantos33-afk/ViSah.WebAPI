using Microsoft.AspNetCore.Http;
using ViSah.Services.Media;

namespace ViSah.Interfaces;

public interface ICoverManagementService
{
    Task<UploadedImage?> UploadTrackCoverAsync(int trackId, IFormFile image, CancellationToken cancellationToken);
    Task<UploadedImage?> UploadAlbumCoverAsync(int albumId, IFormFile image, CancellationToken cancellationToken);
}