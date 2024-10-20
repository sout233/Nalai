using System.Diagnostics;
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
        
        downloader.DownloadProgressChanged += OnDownloadProgressChanged;
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

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var chunks = e.ActiveChunks;
        var progress = e.ProgressPercentage;
        var speed = e.BytesPerSecondSpeed / 1024;
        var remaining = e.TotalBytesToReceive - e.ReceivedBytesSize;
        
        Debug.WriteLine($"Chunks: {chunks}, Progress: {progress}, Speed: {speed}KB/s, Remaining: {remaining} bytes");
    }
}

public enum DownloadStatus
{
    Idle,
    Downloading,
    Completed,
    Failed
}