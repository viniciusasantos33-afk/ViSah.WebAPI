using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViSah.Interfaces;
using ViSah.Services;
using ViSah.Services.Media;

namespace ViSah.Controllers;

[ApiController]
[Route("api/tracks")]
public sealed class MusicController(
    IMusicService musicService,
    IAudioDeliveryService audioDeliveryService,
    IAudioManagementService audioManagementService,
    ICoverManagementService coverService) : ControllerBase
{
    [HttpGet]
    public Task<object> GetTracks([FromQuery] TrackQuery query)
    {
        return musicService.Search(query);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTrack(int id)
    {
        var track = await musicService.GetTrack(id);
        if (track is null)
        {
            return NotFound();
        }

        return Ok(track);
    }

    [HttpGet("{id:int}/audio")]
    public async Task<IActionResult> GetAudio(int id, CancellationToken cancellationToken)
    {
        var audioUrl = await audioDeliveryService.GetAudioUrlAsync(id, cancellationToken);
        if (audioUrl is null)
        {
            return NotFound(new { message = "A faixa não possui uma URL de áudio HTTPS válida." });
        }

        return Redirect(audioUrl);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/audio")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 52 * 1024 * 1024)]
    public async Task<IActionResult> UploadAudio(
        int id,
        IFormFile audio,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadedAudio = await audioManagementService.UploadTrackAudioAsync(id, audio, cancellationToken);
            if (uploadedAudio is null)
            {
                return NotFound(new { message = "Faixa não encontrada." });
            }

            return Ok(new { trackId = id, audioUrl = uploadedAudio.Url });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize]
    [HttpPost("{id:int}/play")]
    public async Task<IActionResult> RecordPlay(int id, CancellationToken cancellationToken)
    {
        var wasRecorded = await musicService.RecordPlayAsync(id, cancellationToken);
        if (!wasRecorded)
        {
            return NotFound();
        }

        return Ok(new { message = "Reprodução registrada." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTrack(TrackInput request, CancellationToken cancellationToken)
    {
        if (request.DurationSeconds <= 0)
        {
            return BadRequest(new { message = "A duração deve ser maior que zero." });
        }

        var track = await musicService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTrack), new { id = track.Id }, track);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTrack(
        int id,
        TrackInput request,
        CancellationToken cancellationToken)
    {
        if (request.DurationSeconds <= 0)
        {
            return BadRequest(new { message = "A duração deve ser maior que zero." });
        }

        var wasUpdated = await musicService.UpdateAsync(id, request, cancellationToken);
        if (!wasUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTrack(int id, CancellationToken cancellationToken)
    {
        var wasDeleted = await musicService.DeleteAsync(id, cancellationToken);
        if (!wasDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(
        int id,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadedImage = await coverService.UploadTrackCoverAsync(id, image, cancellationToken);
            if (uploadedImage is null)
            {
                return NotFound(new { message = "Faixa não encontrada." });
            }

            return Ok(new { trackId = id, uploadedImage.Url });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
