using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CompressorService.Api.Helpers;

public static class ImageExtensions
{
    private static readonly ResizeOptions ResizeOptions = new()
    {
        Size = new Size(400, 400),
        Mode = ResizeMode.Crop,
        Sampler = KnownResamplers.Lanczos3
    };

    private static readonly GraphicsOptions GraphicsOptions = new()
    {
        Antialias = true,
        AlphaCompositionMode = PixelAlphaCompositionMode.DestIn
    };

    public static void ApplyRoundedCorners(this IImageProcessingContext ctx, IPath path) =>
        ctx.SetGraphicsOptions(GraphicsOptions)
            .Fill(Color.White, path);

    public static IImageProcessingContext CropToThumbnail(this IImageProcessingContext ctx, Rectangle cropRectangle) =>
        ctx.Crop(cropRectangle).Resize(ResizeOptions);
}