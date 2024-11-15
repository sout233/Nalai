using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class PostSorcResponse : NalaiResponse<NalaiSorcRequest>;

public class NalaiSorcRequest
{
    [JsonProperty("status")] public string Status { get; set; }
}