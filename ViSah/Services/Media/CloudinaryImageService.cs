using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using ViSah.Interfaces;

namespace ViSah.Services.Media;

public sealed class CloudinaryImageService(IConfiguration configuration) : ICloudinaryImageService
{
    private long MaxImageSizeInBytes =>
        configuration.GetValue<long>("Cloudinary:MaxImageSizeMegabytes", 10) * 1024 * 1024;

    private readonly Lazy<Cloudinary> cloudinary = new(() => CreateCloudinary(configuration));

    private static readonly HashSet<string> AllowedImageTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    public async Task<UploadedImage> UploadAsync(
        IFormFile image,
        string folder,
        CancellationToken cancellationToken)
    {
        await ValidateImageAsync(image, cancellationToken);

        await using var stream = image.OpenReadStream();
        var uploadParameters = new ImageUploadParams
        {
            File = new FileDescription(image.FileName, stream),
            Folder = folder,
            UseFilename = false,
            UniqueFilename = true
        };

        var result = await cloudinary.Value.UploadAsync(uploadParameters, cancellationToken);
        if (result.Error is not null)
        {
            throw new HttpRequestException($"Cloudinary recusou o upload: {result.Error.Message}");
        }

        return new UploadedImage(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId)
    {
        var result = await cloudinary.Value.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        });

        if (result.Error is not null)
        {
            throw new HttpRequestException(
                $"Não foi possível excluir a imagem no Cloudinary: {result.Error.Message}");
        }
    }

    private async Task ValidateImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (image.Length == 0)
        {
            throw new ArgumentException("Selecione um arquivo de imagem.", nameof(image));
        }

        if (image.Length > MaxImageSizeInBytes)
        {
            throw new ArgumentException("A imagem deve ter no máximo 10 MB.", nameof(image));
        }

        if (!AllowedImageTypes.Contains(image.ContentType))
        {
            throw new ArgumentException("Formato inválido. Use JPG, PNG, WEBP ou GIF.", nameof(image));
        }

        await using var stream = image.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await stream.ReadAsync(header, cancellationToken);
        var isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isGif = bytesRead >= 6 &&
                    (System.Text.Encoding.ASCII.GetString(header, 0, 6) is "GIF87a" or "GIF89a");
        var isWebp = bytesRead >= 12 &&
                     System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
                     System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";

        if (!isJpeg && !isPng && !isGif && !isWebp)
        {
            throw new ArgumentException("O arquivo enviado não contém uma imagem válida.", nameof(image));
        }
    }

    private static Cloudinary CreateCloudinary(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException(
                "Configure as credenciais Cloudinary antes de fazer upload de imagens.");
        }

        return new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }
}