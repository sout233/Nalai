using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class PostStopResult
{
    [JsonProperty("success")]
    public bool IsSuccess { get; set; }
}