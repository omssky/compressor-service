using CompressorService.Api.Services.Interfaces;
using ImageMagick;

namespace CompressorService.Api.Services;

public class ImageProcessorMagick : IImageProcessorMagick
{
    public Task<byte[]> OptimizeAsync(byte[] imageData)
    {
        using var ms = new MemoryStream(imageData);
        var optimizer = new ImageOptimizer { OptimalCompression = true };
        optimizer.LosslessCompress(ms);
        return Task.FromResult(ms.ToArray());
    }

    public Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height)
    {
        using var ms = new MemoryStream(imageData);
        using var image = new MagickImage(ms);

        if (width > 0 && height > 0)
        {
            image.Resize(new MagickGeometry((uint)width, (uint)height) { IgnoreAspectRatio = false });
        }

        image.Strip();
        image.SetAttribute("jpeg:sampling-factor", "4:2:0");
        image.Quality = (uint)quality;
        image.ColorSpace = ColorSpace.sRGB;
        image.Format = MagickFormat.Jpeg;
        image.SetAttribute("jpeg:interlace", "Plane");

        using var output = new MemoryStream();
        image.Write(output);
        return Task.FromResult(output.ToArray());
    }
}