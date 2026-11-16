using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using ViSah.Interfaces;

namespace ViSah.Services.Media;

public sealed class CloudinaryAudioService(IConfiguration configuration) : ICloudinaryAudioService
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "audio/aac",
        "audio/flac",
        "audio/mp4",
        "audio/mpeg",
        "audio/ogg",
        "audio/wav",
        "audio/x-flac",
        "audio/x-m4a",
        "audio/x-wav"
    ];

    private readonly Lazy<Cloudinary> cloudinary = new(() => CreateCloudinary(configuration));

    public async Task<UploadedAudio> UploadAsync(IFormFile audio, CancellationToken cancellationToken)
    {
        await ValidateAudioAsync(audio, cancellationToken);

        await using var stream = audio.OpenReadStream();
        var parameters = new VideoUploadParams
        {
            File = new FileDescription(Path.GetFileName(audio.FileName), stream),
            Folder = configuration["Cloudinary:TrackAudioFolder"] ?? "visah/audio",
            UseFilename = false,
            UniqueFilename = true
        };

        var result = await cloudinary.Value.UploadAsync(parameters, cancellationToken);
        if (result.Error is not null)
        {
            throw new HttpRequestException($"Cloudinary recusou o áudio: {result.Error.Message}");
        }

        return new UploadedAudio(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId)
    {
        var result = await cloudinary.Value.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Video
        });

        if (result.Error is not null)
        {
            throw new HttpRequestException($"Não foi possível excluir o áudio no Cloudinary: {result.Error.Message}");
        }
    }

    private async Task ValidateAudioAsync(IFormFile audio, CancellationToken cancellationToken)
    {
        if (audio.Length == 0)
        {
            throw new ArgumentException("Selecione um arquivo de áudio.", nameof(audio));
        }

        var maxSizeMegabytes = configuration.GetValue<long>("Cloudinary:MaxAudioSizeMegabytes", 50);
        if (audio.Length > maxSizeMegabytes * 1024 * 1024)
        {
            throw new ArgumentException($"O áudio deve ter no máximo {maxSizeMegabytes} MB.", nameof(audio));
        }

        if (!AllowedContentTypes.Contains(audio.ContentType))
        {
            throw new ArgumentException("Formato inválido. Use MP3, WAV, FLAC, OGG, AAC ou M4A.", nameof(audio));
        }

        await using var stream = audio.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await stream.ReadAsync(header, cancellationToken);
        if (!HasValidAudioSignature(header, bytesRead))
        {
            throw new ArgumentException("O arquivo enviado não contém um áudio válido.", nameof(audio));
        }
    }

    private static bool HasValidAudioSignature(byte[] header, int bytesRead)
    {
        if (bytesRead >= 3 && header.AsSpan(0, 3).SequenceEqual("ID3"u8))
        {
            return true;
        }

        if (bytesRead >= 2 && header[0] == 0xFF && (header[1] & 0xE0) == 0xE0)
        {
            return true;
        }

        if (bytesRead >= 12 &&
            (System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
             System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WAVE" ||
             System.Text.Encoding.ASCII.GetString(header, 0, 4) == "FORM" &&
             System.Text.Encoding.ASCII.GetString(header, 8, 4) == "AIFF" ||
             System.Text.Encoding.ASCII.GetString(header, 4, 4) == "ftyp"))
        {
            return true;
        }

        return bytesRead >= 4 &&
               (header.AsSpan(0, 4).SequenceEqual("fLaC"u8) ||
                header.AsSpan(0, 4).SequenceEqual("OggS"u8));
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
            throw new InvalidOperationException("Configure as credenciais Cloudinary antes de enviar áudios.");
        }

        return new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }
}