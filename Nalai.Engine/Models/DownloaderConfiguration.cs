namespace Nalai.Engine.Models;

public class DownloaderConfiguration
{
    public int ChunkCount { get; set; } = 8; // 默认分块数量
    public int MaxConcurrentDownloads { get; set; } = 3; // 最大并发下载数量
}