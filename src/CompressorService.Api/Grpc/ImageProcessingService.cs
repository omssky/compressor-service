using CompressorService.Api.Protos;
using CompressorService.Api.Services.Interfaces;
using Google.Protobuf;
using Grpc.Core;

namespace CompressorService.Api.Grpc
{
    public class ImageProcessingService(IImageProcessorImageSharp imageProcessor) : Protos.ImageProcessingService.ImageProcessingServiceBase
    {
        public override async Task<ImageResponse> OptimizeImage(ImageRequest request, ServerCallContext context)
        {
            var inputBytes = request.ImageData.ToByteArray();
            var resultBytes = await imageProcessor.OptimizeAsync(inputBytes);

            return new ImageResponse
            {
                ImageData = ByteString.CopyFrom(resultBytes),
                FileName = request.FileName
            };
        }

        public override async Task<ImageResponse> CompressImage(CompressRequest request, ServerCallContext context)
        {
            var inputBytes = request.ImageData.ToByteArray();
            var resultBytes = await imageProcessor.CompressAsync(inputBytes, request.Quality, request.Width, request.Height);

            return new ImageResponse
            {
                ImageData = ByteString.CopyFrom(resultBytes),
                FileName = request.FileName
            };
        }
    }
}