using CompressorService.Api.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CompressorService.Api.Services;

public class ImageProcessorImageSharp : IImageProcessorImageSharp
{
    public async Task<byte[]> OptimizeAsync(byte[] imageData)
    {
        using var input = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<Rgba32>(input);

        using var output = new MemoryStream();
        var encoder = new WebpEncoder
        {
            Quality = 84,
            Method = WebpEncodingMethod.Level4
        };
        await image.SaveAsWebpAsync(output, encoder);
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
        var encoder = new WebpEncoder
        {
            Quality = quality,
            Method = WebpEncodingMethod.Level4
        };
        await image.SaveAsWebpAsync(output, encoder);
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

        image.Mutate(ctx =>
        {
            ctx.Crop(cropRectangle)
                .Resize(new ResizeOptions
                {
                    Size = new Size(400, 400),
                    Mode = ResizeMode.Crop,
                    Sampler = KnownResamplers.Lanczos3
                });

            ctx.SetGraphicsOptions(new GraphicsOptions
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestIn
            });
            ctx.Fill(Color.White, mask);
        });

        using var output = new MemoryStream();
        var encoder = new WebpEncoder
        {
            Quality = 80,
            Method = WebpEncodingMethod.Level4
        };
        await image.SaveAsWebpAsync(output, encoder);

        return output.ToArray();
    }

    public async Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images)
    {
        var tasks = images.Select(async imageData =>
        {
            using var input = new MemoryStream(imageData);
            using var image = await Image.LoadAsync<Rgba32>(input);

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder
            {
                Quality = 84,
                Method = WebpEncodingMethod.Level4
            });

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
                image.Mutate(ctx =>
                {
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(item.Width, item.Height),
                        Mode = ResizeMode.Max
                    });
                });
            }

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder
            {
                Quality = item.Quality,
                Method = WebpEncodingMethod.Level4
            });

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

            image.Mutate(ctx =>
            {
                ctx.Crop(cropRectangle)
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(400, 400),
                        Mode = ResizeMode.Crop,
                        Sampler = KnownResamplers.Lanczos3
                    });
                ctx.SetGraphicsOptions(new GraphicsOptions
                {
                    Antialias = true,
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestIn
                });
                ctx.Fill(Color.White, mask);
            });

            using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder
            {
                Quality = 80,
                Method = WebpEncodingMethod.Level4
            });

            return output.ToArray();
        });

        return await Task.WhenAll(tasks);
    }
}