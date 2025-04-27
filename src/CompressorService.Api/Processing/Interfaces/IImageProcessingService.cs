namespace CompressorService.Api.Processing.Interfaces;

public interface IImageProcessor
{
    Task<byte[]> OptimizeAsync(byte[] imageData, CancellationToken cancellationToken);
    Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height, CancellationToken cancellationToken);
    Task<byte[]> CreateThumbnailAsync(byte[] imageData, CancellationToken cancellationToken);

    Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images, CancellationToken cancellationToken);
    Task<byte[][]> CompressBatchAsync(IEnumerable<(byte[] ImageData, int Quality, int Width, int Height)> images, CancellationToken cancellationToken);
    Task<byte[][]> CreateThumbnailBatchAsync(IEnumerable<byte[]> images, CancellationToken cancellationToken);
}