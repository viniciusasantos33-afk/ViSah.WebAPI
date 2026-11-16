using Microsoft.EntityFrameworkCore;
using ViSah.Data;
using ViSah.Interfaces;
using ViSah.Services.Media;

namespace ViSah.Services;

public sealed class CoverManagementService(
    AppDbContext db,
    ICloudinaryImageService imageService,
    IConfiguration configuration,
    ILogger<CoverManagementService> logger) : ICoverManagementService
{
    public async Task<UploadedImage?> UploadTrackCoverAsync(
        int trackId,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        var track = await db.Tracks.FindAsync([trackId], cancellationToken);
        if (track is null)
        {
            return null;
        }

        var folder = configuration["Cloudinary:TrackCoversFolder"] ?? "visah/tracks";
        var uploadedImage = await imageService.UploadAsync(image, folder, cancellationToken);
        var previousPublicId = track.CoverPublicId;
        track.CoverUrl = uploadedImage.Url;
        track.CoverPublicId = uploadedImage.PublicId;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TryDeleteUploadedImageAsync(uploadedImage.PublicId);
            throw;
        }

        await TryDeletePreviousImageAsync(previousPublicId);
        return uploadedImage;
    }

    public async Task<UploadedImage?> UploadAlbumCoverAsync(
        int albumId,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        var album = await db.Albums.FindAsync([albumId], cancellationToken);
        if (album is null)
        {
            return null;
        }

        var folder = configuration["Cloudinary:AlbumCoversFolder"] ?? "visah/albums";
        var uploadedImage = await imageService.UploadAsync(image, folder, cancellationToken);
        var previousPublicId = album.CoverPublicId;
        album.CoverUrl = uploadedImage.Url;
        album.CoverPublicId = uploadedImage.PublicId;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TryDeleteUploadedImageAsync(uploadedImage.PublicId);
            throw;
        }

        await TryDeletePreviousImageAsync(previousPublicId);
        return uploadedImage;
    }

    private async Task TryDeletePreviousImageAsync(string? publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        try
        {
            await imageService.DeleteAsync(publicId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Falha ao remover imagem substituída {PublicId} do Cloudinary.", publicId);
        }
    }

    private async Task TryDeleteUploadedImageAsync(string publicId)
    {
        try
        {
            await imageService.DeleteAsync(publicId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Falha ao limpar imagem {PublicId} após erro no banco.", publicId);
        }
    }
}