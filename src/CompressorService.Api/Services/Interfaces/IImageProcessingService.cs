namespace CompressorService.Api.Services.Interfaces;

public interface IImageProcessor
{
    Task<byte[]> OptimizeAsync(byte[] imageData);
    Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height);
    Task<byte[]> CreateThumbnailAsync(byte[] imageData);

    Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images);
    Task<byte[][]> CompressBatchAsync(IEnumerable<(byte[] ImageData, int Quality, int Width, int Height)> images);
    Task<byte[][]> CreateThumbnailBatchAsync(IEnumerable<byte[]> images);
}