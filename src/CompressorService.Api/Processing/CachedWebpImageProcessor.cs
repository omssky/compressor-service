using System.Security.Cryptography;
using CompressorService.Api.Metrics;
using CompressorService.Api.Options;
using CompressorService.Api.Processing.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace CompressorService.Api.Processing;

public class CachedWebpImageProcessor(
    IWebpImageProcessor innerProcessor,
    IMemoryCache cache,
    IOptionsMonitor<CacheOptions> cacheOptions,
    ILogger<CachedWebpImageProcessor> logger)
    : IWebpImageProcessor
{
    public async Task<byte[]> OptimizeAsync(byte[] imageData, CancellationToken cancellationToken) =>
        await GetOrAddAsync(
            GenerateCacheKey(nameof(OptimizeAsync), "", imageData),
            () => innerProcessor.OptimizeAsync(imageData, cancellationToken)
        );

    public async Task<byte[]> CompressAsync(
        byte[] imageData, int quality, int width, int height,
        CancellationToken cancellationToken) =>
        await GetOrAddAsync(
            GenerateCacheKey(nameof(CompressAsync), $"quality={quality},width={width},height={height}", imageData),
            () => innerProcessor.CompressAsync(imageData, quality, width, height, cancellationToken)
        );

    public async Task<byte[]> CreateThumbnailAsync(byte[] imageData, CancellationToken cancellationToken) =>
        await GetOrAddAsync(
            GenerateCacheKey(nameof(CreateThumbnailAsync), "", imageData),
            () => innerProcessor.CreateThumbnailAsync(imageData, cancellationToken)
        );

    public async Task<byte[][]> OptimizeBatchAsync(IEnumerable<byte[]> images, CancellationToken cancellationToken) =>
        await GetOrAddBatchAsync(
            images,
            img => GenerateCacheKey(nameof(OptimizeAsync), "", img),
            innerProcessor.OptimizeBatchAsync,
            cancellationToken
        );

    public async Task<byte[][]> CompressBatchAsync(
        IEnumerable<(byte[] ImageData, int Quality, int Width, int Height)> images,
        CancellationToken cancellationToken) =>
        await GetOrAddBatchAsync(
            images,
            item => GenerateCacheKey(nameof(CompressAsync),
                $"quality={item.Quality},width={item.Width},height={item.Height}", item.ImageData),
            innerProcessor.CompressBatchAsync,
            cancellationToken
        );

    public async Task<byte[][]> CreateThumbnailBatchAsync(IEnumerable<byte[]> images,
        CancellationToken cancellationToken) =>
        await GetOrAddBatchAsync(
            images,
            img => GenerateCacheKey(nameof(CreateThumbnailAsync), "", img),
            innerProcessor.CreateThumbnailBatchAsync,
            cancellationToken
        );

    private string GenerateCacheKey(string methodName, string parameters, byte[] imageData) =>
        $"{cacheOptions.CurrentValue.GetVersionPrefix()}:{methodName}:{parameters}:{Convert.ToHexString(SHA256.HashData(imageData))}";

    private async Task<TResult> GetOrAddAsync<TResult>(string cacheKey, Func<Task<TResult>> processorFunc)
    {
        if (!cacheOptions.CurrentValue.IsEnabled)
            return await processorFunc();

        if (cache.TryGetValue(cacheKey, out TResult? cachedResult) && cachedResult is not null)
        {
            logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
            CacheMetrics.RegisterCacheHit();
            return cachedResult;
        }

        logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
        CacheMetrics.RegisterCacheMiss();
        var result = await processorFunc();

        cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromSeconds(cacheOptions.CurrentValue.ExpirationSeconds)
        });
        return result;
    }

    private async Task<TResult[]> GetOrAddBatchAsync<TInput, TResult>(
        IEnumerable<TInput> items,
        Func<TInput, string> cacheKeyFactory,
        Func<TInput[], CancellationToken, Task<TResult[]>> processorFunc,
        CancellationToken cancellationToken)
    {
        if (!cacheOptions.CurrentValue.IsEnabled)
            return await processorFunc(items.ToArray(), cancellationToken);

        var itemsList = items.ToList();

        var groupedByKeys = itemsList
            .GroupBy(cacheKeyFactory)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var resultDict = new Dictionary<string, TResult>();

        foreach (var kvp in groupedByKeys)
        {
            if (cache.TryGetValue(kvp.Key, out TResult? value) && value is not null)
            {
                resultDict[kvp.Key] = value;
                logger.LogDebug("Cache hit for key {CacheKey}", kvp.Key);
                CacheMetrics.RegisterCacheHit();
            }
            else
            {
                logger.LogDebug("Cache miss for key {CacheKey}", kvp.Key);
                CacheMetrics.RegisterCacheMiss();
            }
        }

        var missingKeys = groupedByKeys.Keys.Except(resultDict.Keys).ToList();

        if (missingKeys.Count == 0)
        {
            return resultDict.Values.ToArray();
        }

        var inputsToProcess = missingKeys.Select(key => groupedByKeys[key].First()).ToArray();
        var processedResults = await processorFunc(inputsToProcess, cancellationToken);

        foreach (var (key, result) in missingKeys.Zip(processedResults, (k, r) => (k, r)))
        {
            resultDict[key] = result;
            cache.Set(key, result, new MemoryCacheEntryOptions
            {
                Size = 1,
                SlidingExpiration = TimeSpan.FromSeconds(cacheOptions.CurrentValue.ExpirationSeconds)
            });
        }

        return resultDict.Values.ToArray();
    }
}