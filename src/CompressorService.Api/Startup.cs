using CompressorService.Api.Services;
using CompressorService.Api.Services.Interfaces;
using Microsoft.OpenApi.Models;

namespace CompressorService.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "CompressorService API", 
                Version = "v1",
                Description = "API для обработки изображений"
            });
            c.OperationFilter<SwaggerFileOperationFilter>();
        });
        
        services.AddTransient<IImageProcessorMagick, ImageProcessorMagick>();
        services.AddTransient<IImageProcessorImageSharp, ImageProcessorImageSharp>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompressorService API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}