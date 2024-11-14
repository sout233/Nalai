using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class NalaiCoreDownloadResult
{
    [JsonProperty("id")] public string? Id { get; set; }
}

public class PostDownloadResponse : NalaiResult<NalaiCoreDownloadResult>;