using CompressorService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompressorService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageProcessingController(
    IImageProcessorMagick magickProcessor,
    IImageProcessorImageSharp imageSharpProcessor)
    : ControllerBase
{
    [HttpPost("optimize/imagemagick")]
    public async Task<IActionResult> OptimizeMagick([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var result = await magickProcessor.OptimizeAsync(fileBytes);
        return File(result, "image/jpeg", file.FileName);
    }

    [HttpPost("optimize/imagesharp")]
    public async Task<IActionResult> OptimizeImageSharp([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var result = await imageSharpProcessor.OptimizeAsync(fileBytes);
        return File(result, "image/jpeg", file.FileName);
    }

    [HttpPost("compress/imagemagick")]
    public async Task<IActionResult> CompressMagick(
        [FromForm] IFormFile? file,
        [FromQuery] int quality = 75,
        [FromQuery] int width = 0,
        [FromQuery] int height = 0)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        if (quality is < 0 or > 100)
            return BadRequest("Quality must be between 0 and 100.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var result = await magickProcessor.CompressAsync(fileBytes, quality, width, height);
        return File(result, "image/jpeg", file.FileName);
    }

    [HttpPost("compress/imagesharp")]
    public async Task<IActionResult> CompressImageSharp(
        [FromForm] IFormFile? file,
        [FromQuery] int quality = 75,
        [FromQuery] int width = 0,
        [FromQuery] int height = 0)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        if (quality is < 0 or > 100)
            return BadRequest("Quality must be between 0 and 100.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var result = await imageSharpProcessor.CompressAsync(fileBytes, quality, width, height);
        return File(result, "image/jpeg", file.FileName);
    }
}
