using Newtonsoft.Json;

namespace Nalai.Models;

public abstract class NativeMessagingConfig(
    string name,
    string description,
    string path,
    string type)
{
    [JsonProperty("name")] public string Name { get; set; } = name;

    [JsonProperty("description")] public string Description { get; set; } = description;

    [JsonProperty("path")] public string Path { get; set; } = path;

    [JsonProperty("type")] public string Type { get; set; } = type;
}

public class ChromeNativeMessagingConfig(
    string name,
    string description,
    string path,
    string type,
    List<string> allowedOrigins) : NativeMessagingConfig(name, description, path, type)
{
    [JsonProperty("allowed_origins")] public List<string> AllowedOrigins { get; set; } = allowedOrigins;
}

public class FirefoxNativeMessagingConfig(
    string name,
    string description,
    string path,
    string type,
    List<string> allowedExtensions) : NativeMessagingConfig(name, description, path, type)
{
    [JsonProperty("allowed_extensions")] public List<string> AllowedExtensions { get; set; } = allowedExtensions;
}