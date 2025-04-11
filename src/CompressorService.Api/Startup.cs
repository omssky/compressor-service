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
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGrpcService<ImageProcessingService>();
            if (env.IsDevelopment())
            {
                endpoints.MapGrpcReflectionService();
            }
        });
    }
}