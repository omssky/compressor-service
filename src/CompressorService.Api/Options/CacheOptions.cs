// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace CompressorService.Api.Options;

public class CacheOptions
{
    public bool IsEnabled { get; set; }
    public int Version { get; set; } = 1;
    public int ExpirationSeconds { get; set; } = 60;

    public string GetVersionPrefix()
    {
        return $"v{Version}";
    }
}