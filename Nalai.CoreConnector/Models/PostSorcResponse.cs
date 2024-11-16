using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class PostSorcResponse : NalaiResponse<NalaiSorcResult>;

public class NalaiSorcResult
{
    [JsonProperty("running")] public bool IsRunning { get; set; }
}