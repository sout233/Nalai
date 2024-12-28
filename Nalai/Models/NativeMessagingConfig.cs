using Newtonsoft.Json;

namespace Nalai.Models;

public record NativeMessagingConfig(
    string Name,
    string Description,
    string Path,
    string Type,
    List<string> AllowedOrigins)
{
    [JsonProperty("name")]
    public string Name { get; set; } = Name;

    [JsonProperty("description")]
    public string Description { get; set; } = Description;

    [JsonProperty("path")]
    public string Path { get; set; } = Path;

    [JsonProperty("type")]
    public string Type { get; set; } = Type;

    [JsonProperty("allowed_origins")]
    public List<string> AllowedOrigins { get; set; } = AllowedOrigins;
}