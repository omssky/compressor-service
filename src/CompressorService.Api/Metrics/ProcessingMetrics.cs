using Prometheus;

namespace CompressorService.Api.Metrics;

public static class ProcessingMetrics
{
    private static readonly Counter ImageCounter = Prometheus.Metrics.CreateCounter(
        "image_processing_image_counter",
        "Total number processed images"
    );
    
    public static void RegisterImage() => ImageCounter.Inc();
    
    public static void RegisterImages(int count) => ImageCounter.Inc(count);
}