using CompressorService.Api.Protos;
using CompressorService.Api.Services.Interfaces;
using Google.Protobuf;
using Grpc.Core;

namespace CompressorService.Api.Grpc
{
    public class ImageProcessingService(IImageProcessorImageSharp imageProcessor) : Protos.ImageProcessingService.ImageProcessingServiceBase
    {
        public override async Task<ProcessedImageResponse> OptimizeImage(OptimizeImageRequest request, ServerCallContext context)
        {
            var resultBytes = await imageProcessor.OptimizeAsync(request.Image.ImageData.ToByteArray());

            return new ProcessedImageResponse
            {
                Image = new Image
                {
                    ImageData = ByteString.CopyFrom(resultBytes),
                    Type = FileType.TypeWebp
                }
            };
        }

        public override async Task<ProcessedImageResponse> CompressImage(CompressImageRequest request, ServerCallContext context)
        {
            var resultBytes = await imageProcessor.CompressAsync(
                request.Image.ImageData.ToByteArray(), 
                request.Params.Quality, 
                request.Params.Width, 
                request.Params.Height
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

        public override async Task<ProcessedImageResponse> CreateThumbnail(CreateThumbnailRequest request, ServerCallContext context)
        {
            var result = await imageProcessor.CreateThumbnailAsync(request.Image.ImageData.ToByteArray());

            return new ProcessedImageResponse
            {
                Image = new Image
                {
                    ImageData = ByteString.CopyFrom(result),
                    Type = FileType.TypeWebp
                }
            };
        }

        public override Task<BatchProcessedImageResponse> OptimizeBatch(BatchOptimizeImageRequest request, ServerCallContext context)
        {
            return base.OptimizeBatch(request, context);
        }

        public override Task<BatchProcessedImageResponse> CompressBatch(BatchCompressImageRequest request, ServerCallContext context)
        {
            return base.CompressBatch(request, context);
        }

        public override Task<BatchProcessedImageResponse> CreateThumbnailBatch(BatchThumbnailRequest request, ServerCallContext context)
        {
            return base.CreateThumbnailBatch(request, context);
        }
    }
}