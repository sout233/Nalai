using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class NalaiStopResult
{
    [JsonProperty("success")]
    public bool IsSuccess { get; set; }
}

public class PostStopResponse : NalaiResponse<NalaiStopResult>;