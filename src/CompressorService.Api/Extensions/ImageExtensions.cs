using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CompressorService.Api.Extensions;

public static class ImageExtensions
{
    public static void ApplyRoundedCorners(this IImageProcessingContext ctx, IPath path)
    {
        ctx.SetGraphicsOptions(new GraphicsOptions
        {
            Antialias = true,
            AlphaCompositionMode = PixelAlphaCompositionMode.DestIn
        });

        ctx.Fill(Color.White, path);
    }
}