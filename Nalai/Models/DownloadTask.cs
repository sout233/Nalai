using System.IO;
using Downloader;

namespace Nalai.Models;

public class DownloadTask
{
    public string Url { get; set; }
    public string FileName { get; set; }
    public string DownloadPath { get; set; }
    public string Id { get; set; }
    public DownloadStatus Status { get; set; }
    public DownloadConfiguration DownloadOpt { get; set; }
    public DownloadService Downloader { get; set; }
    
    
    public DownloadTask(string url, string fileName, string path)
    {
        Url = url;
        FileName = fileName;
        DownloadPath = path;
        Status = DownloadStatus.Idle;
        
        var downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 8, 
            ParallelDownload = true 
        };

        var downloader = new DownloadService(downloadOpt);
        
        DownloadOpt = downloadOpt;
        Downloader = downloader;
    }
    
    public void UpdateStatus(DownloadStatus status)
    {
        Status = status;
    }

    public async Task StartDownload()
    {
        DirectoryInfo path = new DirectoryInfo(DownloadPath);
        await Downloader.DownloadFileTaskAsync(this.Url, Path.Combine(path.FullName, FileName));
    }
}

public enum DownloadStatus
{
    Idle,
    Downloading,
    Completed,
    Failed
}