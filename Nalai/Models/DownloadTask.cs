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

        Downloader.DownloadProgressChanged += OnDownloadProgressChanged;
        Downloader.DownloadStarted += OnDownloadStarted;
        Downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
        Downloader.DownloadFileCompleted += OnDownloadFileCompleted;
    }

    public void UpdateStatus(DownloadStatus status)
    {
        Status = status;
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
        var chunks = e.ActiveChunks;
        var progress = e.ProgressPercentage;
        var speed = e.BytesPerSecondSpeed / 1024;
        var remaining = e.TotalBytesToReceive - e.ReceivedBytesSize;

        Debug.WriteLine($"Chunks: {chunks}, Progress: {progress}, Speed: {speed}KB/s, Remaining: {remaining} bytes");
    }
    
    private void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        Status = DownloadStatus.Completed;
        NalaiMsgBox.Show("Download completed");
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