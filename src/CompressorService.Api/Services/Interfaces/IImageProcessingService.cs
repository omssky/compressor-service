namespace CompressorService.Api.Services.Interfaces;

public interface IImageProcessor
{
    /// <summary>
    /// Performs optimization (compression) of the image.
    /// </summary>
    Task<byte[]> OptimizeAsync(byte[] imageData);

    /// <summary>
    /// Compresses the image with resizing and quality params.
    /// </summary>
    Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height);
    
    /// <summary>
    /// Creates a 400x400 round thumbnail in webp format.
    /// </summary>
    Task<byte[]> CreateThumbnailAsync(byte[] imageData);
}