using CompressorService.Api.Extensions;
using CompressorService.Api.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats.Jpeg;
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
        var encoder = new JpegEncoder
        {
            SkipMetadata = true,
            Quality = 82,
            Interleaved = true,
            ColorType = JpegEncodingColor.YCbCrRatio420
        };
        await image.SaveAsJpegAsync(output, encoder);
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
        var encoder = new JpegEncoder
        {
            SkipMetadata = true,
            Quality = quality,
            Interleaved = true,
            ColorType = JpegEncodingColor.YCbCrRatio420
        };
        await image.SaveAsJpegAsync(output, encoder);
        return output.ToArray();
    }

    public async Task<byte[]> CreateThumbnailAsync(byte[] imageData)
    {
        using var input = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<Rgba32>(input);

        image.Mutate(x =>
        {
            x.Resize(new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Crop
            });

            var mask = new EllipsePolygon(200, 200, 200);
            x.ApplyRoundedCorners(mask);
        });

        using var output = new MemoryStream();
        var encoder = new WebpEncoder
        {
            Quality = 80
        };
        await image.SaveAsWebpAsync(output, encoder);
        return output.ToArray();
    }
}
