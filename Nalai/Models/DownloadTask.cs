using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.Models;

public class DownloadTask
{
    public string Url { get; set; }
    public string FileName { get; set; }
    public string DownloadPath { get; set; }
    public string Id { get; set; }
    public string StatusText { get; set; } = "等待中...";

    public long TotalBytesToReceive { get; set; }
    public float Progress { get; set; }
    public string FileSizeText { get; set; }

    private DownloadStatus _status;

    public DownloadStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            StatusText = value switch
            {
                DownloadStatus.Created => "连接中...",
                DownloadStatus.Running => Progress.ToString("0.00") + "%",
                DownloadStatus.Completed => "已完成",
                DownloadStatus.Failed => "下载失败",
                DownloadStatus.Paused => "已暂停",
                DownloadStatus.None => "等待中...",
                DownloadStatus.Stopped => "已停止",
                _ => StatusText
            };
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public DownloadConfiguration DownloadOpt { get; set; }

    public DownloadService Downloader { get; set; }

    public DownloadPackage Package { get; set; }

    public event EventHandler<EventArgs> StatusChanged;

    public List<Window> BindWindows { get; set; } = [];

    public DownloadTask(string url, string fileName, string path)
    {
        Url = url;
        FileName = fileName;
        DownloadPath = path;
        Status = DownloadStatus.Created;

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

    // TODO: 下载状态获取不正确
    public DownloadStatus PauseOrResume()
    {
        if (Downloader.Status is DownloadStatus.Completed or DownloadStatus.Failed)
        {
            return Downloader.Status;
        }

        if (Downloader.Status is DownloadStatus.Stopped)
        {
            Downloader.DownloadFileTaskAsync(Package);
            Downloader.Resume();
            return DownloadStatus.Running;
        }

        if (Downloader.IsPaused)
        {
            Downloader.Resume();
            Status = DownloadStatus.Running;
            Console.WriteLine($"{FileName} Resumed");
            foreach (var window in BindWindows)
            {
                if (window is DownloadingWindow dw)
                {
                    dw.ViewModel.UpdatePausedOrResumeBtn();
                }
            }
        }
        else
        {
            Downloader.Pause();
            Status = DownloadStatus.Paused;
            Console.WriteLine($"{FileName} Paused");
            foreach (var window in BindWindows)
            {
                if (window is DownloadingWindow dw)
                {
                    dw.ViewModel.UpdatePausedOrResumeBtn();
                }
            }
        }

        return Downloader.Status;
    }


    private void UpdateStatus()
    {
        Status = Downloader.Status;
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        Progress = (float)e.ProgressPercentage;

        var chunks = e.ActiveChunks;
        var progress = e.ProgressPercentage;
        var speed = e.BytesPerSecondSpeed / 1024;
        var remaining = e.TotalBytesToReceive - e.ReceivedBytesSize;

        var fileSize = ByteSizeFormatter.FormatSize(e.TotalBytesToReceive);
        var downloadedSize = ByteSizeFormatter.FormatSize(e.ReceivedBytesSize);

        FileSizeText = $"{downloadedSize} / {fileSize}";

        TotalBytesToReceive = e.TotalBytesToReceive;

        UpdateStatus();

        Debug.WriteLine($"Chunks: {chunks}, Progress: {progress}, Speed: {speed}KB/s, Remaining: {remaining} bytes");
    }

    private void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        UpdateStatus();
        if (e.Error == null)
        {
            FileSizeText = ByteSizeFormatter.FormatSize(TotalBytesToReceive);
            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = new DownloadCompleteWindowViewModel(this);
                var window = new DownloadCompleteWindow(vm, this);
                vm.BindWindow = window;
                window.Show();
                NalaiDownService.CloseTaskBindWindows(this);
            });
        }
        else if (e.Cancelled)
        {
            Status = DownloadStatus.Stopped;
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
        UpdateStatus();
    }

    // public void Cancel()
    // {
    //     Package = Downloader.Package;
    //     Downloader.CancelAsync();
    //     Status = DownloadStatus.Stopped;
    // }
}