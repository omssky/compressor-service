using CompressorService.Api.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CompressorService.Api.Services;

public class ImageProcessorImageSharp : IImageProcessorImageSharp
{
    public async Task<byte[]> OptimizeAsync(byte[] imageData)
    {
        using var input = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(input);

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
        using var image = await Image.LoadAsync(input);

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
            ColorType = JpegEncodingColor.YCbCrRatio420,
            Interleaved = true
        };
        await image.SaveAsJpegAsync(output, encoder);
        return output.ToArray();
    }
}