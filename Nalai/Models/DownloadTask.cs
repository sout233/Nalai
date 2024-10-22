using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;

namespace Nalai.Models;

public class DownloadTask
{
    public string Url { get; set; }
    public string FileName { get; set; }
    public string DownloadPath { get; set; }
    public string Id { get; set; }
    public string StatusText { get; set; } = "等待中...";

    public float Progress { get; set; }
    
    private DownloadStatus _status;
    public DownloadStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            StatusText = value switch
            {
                DownloadStatus.Idle => "连接中...",
                DownloadStatus.Downloading => Progress.ToString("0.00") + "%",
                DownloadStatus.Completed => "已完成",
                DownloadStatus.Failed => "下载失败",
                _ => StatusText
            };
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public DownloadConfiguration DownloadOpt { get; set; }
    public DownloadService Downloader { get; set; }
    
    public event EventHandler<EventArgs> StatusChanged;

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

        Downloader.DownloadProgressChanged += OnDownloadProgressChanged;
        Downloader.DownloadStarted += OnDownloadStarted;
        Downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
        Downloader.DownloadFileCompleted += OnDownloadFileCompleted;
    }

    public async Task StartDownload()
    {
        try
        {
            var path = new DirectoryInfo(DownloadPath);
            await Downloader.DownloadFileTaskAsync(this.Url, Path.Combine(path.FullName, FileName));
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
            Status = DownloadStatus.Failed;
        }
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        Progress = (float)e.ProgressPercentage;
    
        var chunks = e.ActiveChunks;
        var progress = e.ProgressPercentage;
        var speed = e.BytesPerSecondSpeed / 1024;
        var remaining = e.TotalBytesToReceive - e.ReceivedBytesSize;

        Status = DownloadStatus.Downloading;
        Debug.WriteLine($"Chunks: {chunks}, Progress: {progress}, Speed: {speed}KB/s, Remaining: {remaining} bytes");
    }

    private void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        Status = DownloadStatus.Completed;
        if (e.Error == null)
        {
            NalaiMsgBox.Show("Download completed");
        }
        else
        {
            NalaiMsgBox.Show(e.Error.Message, "Download failed!");
        }
        
    }

    private void OnChunkDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
    }

    private void OnDownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        Status = DownloadStatus.Downloading;
    }
}

public enum DownloadStatus
{
    Idle,
    Downloading,
    Completed,
    Failed
}