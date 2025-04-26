using Prometheus;

namespace CompressorService.Api.Metrics;

public static class CacheMetrics
{
    private static readonly Counter Hits = Prometheus.Metrics.CreateCounter("image_processing_cache_hits_total",
        "Total number of cache hits in image processing");

    private static readonly Counter Misses = Prometheus.Metrics.CreateCounter("image_processing_cache_misses_total",
        "Total number of cache misses in image processing");

    public static void RegisterCacheHit() => Hits.Inc();
    public static void RegisterCacheMiss() => Misses.Inc();
}