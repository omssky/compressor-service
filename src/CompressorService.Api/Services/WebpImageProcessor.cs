﻿using CompressorService.Api.Helpers;
using CompressorService.Api.Options;
using CompressorService.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CompressorService.Api.Services;

public class WebpImageProcessor(IOptionsMonitor<WebpEncoderOptions> encoderOptions) : IWebpImageProcessor
{
    private WebpEncoder CreateEncoder(int? overrideQuality = null)
    {
        var opt = encoderOptions.CurrentValue;
        return new WebpEncoder
        {
            SkipMetadata = opt.SkipMetadata,
            FileFormat = opt.FileFormat,
            Quality = overrideQuality ?? opt.Quality,
            Method = opt.Method,
            UseAlphaCompression = opt.UseAlphaCompression,
            EntropyPasses = opt.EntropyPasses,
            SpatialNoiseShaping = opt.SpatialNoiseShaping,
            FilterStrength = opt.FilterStrength,
            TransparentColorMode = opt.TransparentColorMode,
            NearLossless = opt.NearLossless,
            NearLosslessQuality = opt.NearLosslessQuality,
        };
    }

    public async Task<byte[]> OptimizeAsync(byte[] imageData)
    {
        using var input = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<Rgba32>(input);

        using var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, CreateEncoder(84));

        return output.ToArray();
    }

    public async Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height)
    {
        using var input = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<Rgba32>(input);

        if (width > 0 && height > 0)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));
        }

        using var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, CreateEncoder(quality));

        return output.ToArray();
    }

    public async Task<byte[]> CreateThumbnailAsync(byte[] imageData)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<Rgba32>(inputStream);

        var size = Math.Min(image.Width, image.Height);
        var cropRectangle = new Rectangle(
            (image.Width - size) / 2,
            (image.Height - size) / 2,
            size,
            size);

        var mask = new EllipsePolygon(200, 200, 200);

        image.Mutate(ctx => ctx
            .CropToThumbnail(cropRectangle)
            .ApplyRoundedCorners(mask)
        );

        using var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, CreateEncoder(84));

        return output.ToArray();
    }
    // TODO CancellationToken ВЕЗДЕ
    public async Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images)
    {
        var tasks = images.Select(async imageData =>
        {
            using var input = new MemoryStream(imageData);
            using var image = await Image.LoadAsync<Rgba32>(input);

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, CreateEncoder(84));

            return output.ToArray();
        });

        return await Task.WhenAll(tasks);
    }


    public async Task<byte[][]> CompressBatchAsync(IEnumerable<(byte[] ImageData, int Quality, int Width, int Height)> images)
    {
        var tasks = images.Select(async item =>
        {
            using var input = new MemoryStream(item.ImageData);
            using var image = await Image.LoadAsync<Rgba32>(input);

            if (item is { Width: > 0, Height: > 0 })
            {
                image.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(item.Width, item.Height),
                    Mode = ResizeMode.Max
                }));
            }

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, CreateEncoder(item.Quality));
            return output.ToArray();
        });

        return await Task.WhenAll(tasks);
    }

    public async Task<byte[][]> CreateThumbnailBatchAsync(IEnumerable<byte[]> images)
    {
        var mask = new EllipsePolygon(200, 200, 200);

        var tasks = images.Select(async imageData =>
        {
            using var input = new MemoryStream(imageData);
            using var image = await Image.LoadAsync<Rgba32>(input);

            var size = Math.Min(image.Width, image.Height);
            var cropRectangle = new Rectangle(
                (image.Width - size) / 2,
                (image.Height - size) / 2,
                size, size);

            image.Mutate(ctx => ctx
                .CropToThumbnail(cropRectangle)
                .ApplyRoundedCorners(mask)
            );

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, CreateEncoder());

            return output.ToArray();
        });

        return await Task.WhenAll(tasks);
    }
}