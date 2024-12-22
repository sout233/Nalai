using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class NalaiCoreDownloadResult
{
    [JsonProperty("id")] public required string Id { get; set; }
}

public class PostDownloadResponse : NalaiResponse<NalaiCoreDownloadResult>;