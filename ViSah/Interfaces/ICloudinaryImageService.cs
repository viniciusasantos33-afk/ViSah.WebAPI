using Microsoft.AspNetCore.Http;

namespace ViSah.Interfaces;

public sealed record UploadedImage(string Url, string PublicId);

public interface ICloudinaryImageService
{
    Task<UploadedImage> UploadAsync(IFormFile image, string folder, CancellationToken cancellationToken);
    Task DeleteAsync(string publicId);
}