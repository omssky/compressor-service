using CompressorService.Api.Protos;
using CompressorService.Api.Services.Interfaces;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace CompressorService.Api.Grpc;

[ApiExplorerSettings(GroupName = "grpc")]
public class ImageProcessingService(IWebpImageProcessor webpImageProcessor)
    : Protos.ImageProcessingService.ImageProcessingServiceBase
{
    public override async Task<ProcessedImageResponse> OptimizeImage(
        OptimizeImageRequest request,
        ServerCallContext context)
    {
        var resultBytes =
            await webpImageProcessor.OptimizeAsync(request.Image.ImageData.ToByteArray(), context.CancellationToken);

        return new ProcessedImageResponse
        {
            Image = new Image
            {
                ImageData = ByteString.CopyFrom(resultBytes),
                Type = FileType.TypeWebp
            }
        };
    }

    public override async Task<ProcessedImageResponse> CompressImage(
        CompressImageRequest request,
        ServerCallContext context)
    {
        var resultBytes = await webpImageProcessor.CompressAsync(
            request.Image.ImageData.ToByteArray(),
            request.Params.Quality,
            request.Params.Width,
            request.Params.Height,
            context.CancellationToken
        );

        return new ProcessedImageResponse
        {
            Image = new Image
            {
                ImageData = ByteString.CopyFrom(resultBytes),
                Type = FileType.TypeWebp
            }
        };
    }

    public override async Task<ProcessedImageResponse> CreateThumbnail(
        CreateThumbnailRequest request,
        ServerCallContext context)
    {
        var result =
            await webpImageProcessor.CreateThumbnailAsync(request.Image.ImageData.ToByteArray(),
                context.CancellationToken);

        return new ProcessedImageResponse
        {
            Image = new Image
            {
                ImageData = ByteString.CopyFrom(result),
                Type = FileType.TypeWebp
            }
        };
    }

    public override async Task<BatchProcessedImageResponse> OptimizeBatch(
        BatchOptimizeImageRequest request,
        ServerCallContext context)
    {
        var imageBytes = request.Images.Select(i => i.ImageData.ToByteArray());
        var results = await webpImageProcessor.OptimizeBatchAsync(imageBytes, context.CancellationToken);

        var response = new BatchProcessedImageResponse();
        response.Images.AddRange(results.Select(r => new Image
        {
            ImageData = ByteString.CopyFrom(r),
            Type = FileType.TypeWebp
        }));

        return response;
    }

    public override async Task<BatchProcessedImageResponse> CompressBatch(
        BatchCompressImageRequest request,
        ServerCallContext context)
    {
        var results = await webpImageProcessor.CompressBatchAsync(
            request.Items.Select(i => (
                ImageData: i.Image.ImageData.ToByteArray(),
                Quality: i.Params.Quality,
                Width: i.Params.Width,
                Height: i.Params.Height
            )), context.CancellationToken);

        var response = new BatchProcessedImageResponse();
        response.Images.AddRange(results.Select(r => new Image
        {
            ImageData = ByteString.CopyFrom(r),
            Type = FileType.TypeWebp
        }));

        return response;
    }

    public override async Task<BatchProcessedImageResponse> CreateThumbnailBatch(
        BatchThumbnailRequest request,
        ServerCallContext context)
    {
        var imageBytes = request.Images.Select(i => i.ImageData.ToByteArray());
        var results = await webpImageProcessor.CreateThumbnailBatchAsync(imageBytes, context.CancellationToken);

        var response = new BatchProcessedImageResponse();
        response.Images.AddRange(results.Select(r => new Image
        {
            ImageData = ByteString.CopyFrom(r),
            Type = FileType.TypeWebp
        }));

        return response;
    }
}