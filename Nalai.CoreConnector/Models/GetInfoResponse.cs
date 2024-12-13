using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public record NalaiCoreInfo
{
    private string _statusText;

    /// <summary>
    /// 已下载字节数
    /// </summary>
    [JsonProperty("downloaded_bytes")]
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 总字节数
    /// </summary>
    [JsonProperty("total_size")]
    public long TotalBytes { get; set; }

    [JsonProperty("speed")] public long BytesPerSecondSpeed { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonProperty("file_name")]
    public string FileName { get; set; }

    /// <summary>
    /// 不解释（
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// 下载状态，详情见core源代码
    /// </summary>
    [JsonProperty("status")]
    public DownloadStatus Status { get; set; }
    
    
    [JsonProperty("save_dir")]
    public string SaveDirectory { get; set; }
    
    [JsonProperty("create_time")]
    public RustSystemTime CreatedTime { get;set; }

    [JsonProperty("chunks")]
    public List<ChunksItem> Chunks { get;set; }
}

public class GetInfoResponse : NalaiResponse<NalaiCoreInfo>;