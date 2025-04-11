﻿using CompressorService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompressorService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "http")]
public class ImageProcessingController(IImageProcessorImageSharp processor) : ControllerBase
{
    [HttpPost("optimize")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Optimize(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await processor.OptimizeAsync(ms.ToArray());
        return File(result, "image/webp", "optimized.webp");
    }

    [HttpPost("compress")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Compress(IFormFile file, int quality, int width, int height)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await processor.CompressAsync(ms.ToArray(), quality, width, height);
        return File(result, "image/webp", "compressed.webp");
    }

    [HttpPost("thumbnail")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Thumbnail(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await processor.CreateThumbnailAsync(ms.ToArray());
        return File(result, "image/webp", "thumbnail.webp");
    }

    [HttpPost("optimize-batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> OptimizeBatch(List<IFormFile> files)
    {
        var tasks = files.Select(async file =>
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return await processor.OptimizeAsync(ms.ToArray());
        });

        var results = await Task.WhenAll(tasks);
        return File(CreateZip(results), "application/zip", "optimized_batch.zip");
    }

    [HttpPost("compress-batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CompressBatch(List<IFormFile> files, int quality, int width, int height)
    {
        var tasks = files.Select(async file =>
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return await processor.CompressAsync(ms.ToArray(), quality, width, height);
        });

        var results = await Task.WhenAll(tasks);
        return File(CreateZip(results), "application/zip", "compressed_batch.zip");
    }

    [HttpPost("thumbnail-batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ThumbnailBatch(List<IFormFile> files)
    {
        var tasks = files.Select(async file =>
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return await processor.CreateThumbnailAsync(ms.ToArray());
        });

        var results = await Task.WhenAll(tasks);
        return File(CreateZip(results), "application/zip", "thumbnails_batch.zip");
    }

    private static byte[] CreateZip(byte[][] images)
    {
        using var archiveStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            for (var i = 0; i < images.Length; i++)
            {
                var entry = archive.CreateEntry($"image_{i + 1}.webp");
                using var entryStream = entry.Open();
                entryStream.Write(images[i]);
            }
        }
        return archiveStream.ToArray();
    }
}
