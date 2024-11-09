using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class PostDownloadResult
{
    [JsonProperty("id")] public string? Id { get; set; }
}