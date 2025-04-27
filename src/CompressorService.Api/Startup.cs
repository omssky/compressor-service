using CompressorService.Api.Grpc;
using CompressorService.Api.Helpers;
using CompressorService.Api.Options;
using CompressorService.Api.Processing;
using CompressorService.Api.Processing.Interfaces;
using Microsoft.OpenApi.Models;
using Prometheus;

namespace CompressorService.Api;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .Configure<WebpEncoderOptions>(configuration.GetSection(nameof(WebpEncoderOptions)))
            .Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));

        services
            .AddSingleton<IWebpImageProcessor, WebpImageProcessor>()
            .Decorate<IWebpImageProcessor, CachedWebpImageProcessor>()
            .Decorate<IWebpImageProcessor, MetricsWebpImageProcessor>();

        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 10_000;
            options.ExpirationScanFrequency = TimeSpan.FromSeconds(10);
            options.CompactionPercentage = 0.1;
        });

        services
            .AddGrpc()
            .AddJsonTranscoding();
        services
            .AddGrpcReflection();

        services
            .AddGrpcSwagger()
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("http", new OpenApiInfo
                {
                    Title = "HTTP API",
                    Version = "v1",
                    Description = "HTTP endpoints for image processing"
                });
                c.SwaggerDoc("grpc", new OpenApiInfo
                {
                    Title = "gRPC API",
                    Version = "v1",
                    Description = "gRPC endpoints for image processing"
                });
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    return docName switch
                    {
                        "http" => apiDesc.GroupName == "http",
                        "grpc" => apiDesc.GroupName == "grpc",
                        _ => false
                    };
                });
                c.OperationFilter<SwaggerFileOperationFilter>();
            });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/http/swagger.json", "HTTP API");
                c.SwaggerEndpoint("/swagger/grpc/swagger.json", "gRPC API");
                c.RoutePrefix = "swagger";
            });
        }
        app.UseRouting();
        app.UseGrpcMetrics();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGrpcService<ImageProcessingService>();
            endpoints.MapMetrics();
            if (env.IsDevelopment())
            {
                endpoints.MapGrpcReflectionService();
            }
        });
    }
}