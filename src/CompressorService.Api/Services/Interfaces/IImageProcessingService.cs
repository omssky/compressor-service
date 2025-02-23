namespace CompressorService.Api.Services.Interfaces;

public interface IImageProcessor
{
    /// <summary>
    /// Performs optimization (lossless-compression) of the image.
    /// </summary>
    Task<byte[]> OptimizeAsync(byte[] imageData);

    /// <summary>
    /// Compresses the image with resizing and quality.
    /// </summary>
    Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height);
}