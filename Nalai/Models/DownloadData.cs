using Newtonsoft.Json;

namespace Nalai.Models;

public class BrowserInfo
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("headers")] public Dictionary<string, string> Headers { get; set; }
}

public class DownloadData
{
    [JsonProperty("version")] public string Version { get; set; }

    [JsonProperty("browser")] public BrowserInfo Browser { get; set; }

    [JsonProperty("url")] public string DownloadUrl { get; set; }
}