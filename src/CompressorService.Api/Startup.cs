using CompressorService.Api.Grpc;
using CompressorService.Api.Services;
using CompressorService.Api.Services.Interfaces;
using Microsoft.OpenApi.Models;

namespace CompressorService.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IImageProcessorImageSharp, ImageProcessorImageSharp>();
        
        services
            .AddGrpc()
            .AddJsonTranscoding();
        services.AddGrpcReflection();

        services.AddGrpcSwagger();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "CompressorService API", 
                Version = "v1",
                Description = "API для обработки изображений"
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompressorService API v1");
                c.RoutePrefix = "swagger";
            });
        }
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ImageProcessingService>();
            if (env.IsDevelopment())
            {
                endpoints.MapGrpcReflectionService();
            }
        });
    }
}