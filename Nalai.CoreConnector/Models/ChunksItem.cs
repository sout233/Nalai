using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class ChunksItem
{
    [JsonProperty("index")]
    public int Index { get;set; }

    [JsonProperty("size")]
    public int Size { get;set; }

    [JsonProperty("downloaded_bytes")]
    public int DownloadedBytes { get;set; }
}