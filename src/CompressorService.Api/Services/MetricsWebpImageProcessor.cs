using CompressorService.Api.Metrics;
using CompressorService.Api.Services.Interfaces;

namespace CompressorService.Api.Services;

// ReSharper disable ClassNeverInstantiated.Global

public class MetricsWebpImageProcessor(IWebpImageProcessor inner) : IWebpImageProcessor
{
    public async Task<byte[]> OptimizeAsync(byte[] imageData, CancellationToken ct)
    {
        ProcessingMetrics.RegisterImage();
        return await inner.OptimizeAsync(imageData, ct);
    }

    public async Task<byte[]> CompressAsync(byte[] imageData, int quality, int width, int height, CancellationToken ct)
    {
        ProcessingMetrics.RegisterImage();
        return await inner.CompressAsync(imageData, quality, width, height, ct);
    }

    public async Task<byte[]> CreateThumbnailAsync(byte[] imageData, CancellationToken ct)
    {
        ProcessingMetrics.RegisterImage();
        return await inner.CreateThumbnailAsync(imageData, ct);
    }

    public async Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images, CancellationToken ct)
    {
        var arr = images.ToArray();
        ProcessingMetrics.RegisterImages(arr.Length);
        return await inner.OptimizeBatchAsync(arr, ct);
    }

    public async Task<byte[][]> CompressBatchAsync(IEnumerable<(byte[] ImageData, int Quality, int Width, int Height)> images, CancellationToken ct)
    {
        var arr = images.ToArray();
        ProcessingMetrics.RegisterImages(arr.Length);
        return await inner.CompressBatchAsync(arr, ct);
    }

    public async Task<byte[][]> CreateThumbnailBatchAsync(IEnumerable<byte[]> images, CancellationToken ct)
    {
        var arr = images.ToArray();
        ProcessingMetrics.RegisterImages(arr.Length);
        return await inner.CreateThumbnailBatchAsync(arr, ct);
    }
}