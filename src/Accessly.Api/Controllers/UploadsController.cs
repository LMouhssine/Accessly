using Accessly.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize(Roles = Roles.OrganizerOrAdmin)]
public sealed class UploadsController(IFileStorage storage) : ControllerBase
{
    private static readonly string[] AllowedContentTypes = ["image/png", "image/jpeg", "image/webp", "image/gif"];

    /// <summary>Uploads an image (for example an event cover) and returns its public URL.</summary>
    [HttpPost("image")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<UploadResponse>> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was provided.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest("Unsupported image type.");
        }

        await using var stream = file.OpenReadStream();
        var url = await storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(new UploadResponse(url));
    }
}

public sealed record UploadResponse(string Url);
