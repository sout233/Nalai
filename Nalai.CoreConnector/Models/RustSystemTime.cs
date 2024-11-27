using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class RustSystemTime
{
    [JsonProperty("secs_since_epoch")]
    public long SecsSinceEpoch { get;set; }
    
    [JsonProperty("nanos_since_epoch")]
    public long NanosSinceEpoch { get;set; }
}