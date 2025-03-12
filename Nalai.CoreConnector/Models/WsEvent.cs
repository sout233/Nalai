using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class WsEvent<T>
{
    [JsonProperty("event_type")] public required string EventType { get; set; }

    [JsonProperty("id")] public string? Id { get; set; }

    [JsonProperty("data")] public T? Data { get; set; }
}