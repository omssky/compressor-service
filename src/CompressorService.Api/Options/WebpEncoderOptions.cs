using SixLabors.ImageSharp.Formats.Webp;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace CompressorService.Api.Options;

public class WebpEncoderOptions
{
    public int Quality { get; set; } = 80;
    public bool SkipMetadata { get; set; } = true;
    public WebpFileFormatType FileFormat { get; set; } = WebpFileFormatType.Lossy;
    public WebpEncodingMethod Method { get; set; } = WebpEncodingMethod.Level3;
    public bool UseAlphaCompression { get; set; } = false;
    public int EntropyPasses { get; set; } = 1;
    public int SpatialNoiseShaping { get; set; } = 0;
    public int FilterStrength { get; set; } = 20;
    public WebpTransparentColorMode TransparentColorMode { get; set; } = WebpTransparentColorMode.Clear;
    public bool NearLossless { get; set; } = false;
    public int NearLosslessQuality { get; set; } = 100;
}