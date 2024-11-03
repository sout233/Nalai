using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;
using Newtonsoft.Json;
using SqlSugar;

namespace Nalai.Models;

public class DownloadTask
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public long Key { get; set; }

    public string Url { get; set; } = null!;

    [SugarColumn(IsNullable = true)] public string FileName { get; set; }

    public string DownloadPath { get; set; } = null!;

    public string StatusText { get; set; } = "等待中...";

    public string DownloaderJson { get; set; } = null!;

    [SugarColumn(IsNullable = true)] public long TotalBytesToReceive { get; set; }

    [SugarColumn(IsNullable = true)] public float Progress { get; set; }

    [SugarColumn(IsNullable = true)] public string FileSizeText { get; set; } = "Unknown";

    private DownloadStatus _status;

    private DownloadPackage? _package;

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

    [SugarColumn(IsIgnore = true)] public DownloadConfiguration DownloadOpt { get; set; }

    [SugarColumn(IsIgnore = true)] public DownloadService? Downloader { get; set; }


    [SugarColumn(IsIgnore = true)]
    public DownloadPackage? Package
    {
        get
        {
            if (_package is null)
            {
                var path = Path.Combine(DownloadPath, FileName + ".nalai!");
                if (!File.Exists(path))
                {
                    return null;
                }

                var packageJson = File.ReadAllText(path);
                _package = JsonConvert.DeserializeObject<DownloadPackage>(packageJson);
            }

            return _package;
        }
        set
        {
            _package = value;
            var packageJson = JsonConvert.SerializeObject(value);
            File.WriteAllText(Path.Combine(DownloadPath, FileName + ".nalai!"), packageJson);
        }
    }

    public event EventHandler<EventArgs>? StatusChanged;

    [SugarColumn(IsIgnore = true)] public List<Window> BindWindows { get; set; } = [];

    public DownloadTask(string url, string fileName, string path)
    {
        Url = url;
        FileName = fileName;
        DownloadPath = path;
        Status = DownloadStatus.Created;
        Key = SnowFlakeSingle.Instance.NextId();

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

        DownloaderJson = JsonConvert.SerializeObject(downloader);
    }

    public DownloadTask()
    {
        // For SqlSugar
        Url = Url;
    }


    public async Task StartDownload()
    {
        var path = new DirectoryInfo(DownloadPath);
        await Downloader?.DownloadFileTaskAsync(Url, Path.Combine(path.FullName, FileName))!;
        // try
        // {
        //     var path = new DirectoryInfo(DownloadPath);
        //     await Downloader?.DownloadFileTaskAsync(this.Url, Path.Combine(path.FullName, FileName))!;
        // }
        // catch (Exception ex)
        // {
        //     NalaiMsgBox.Show(ex.Message, "Error");
        //     Status = DownloadStatus.Failed;
        // }
    }
    

    // TODO: 下载状态获取不正确
    public DownloadStatus PauseOrResume()
    {
        if (Downloader is null)
        {
            using var downloader = new DownloadService(new DownloadConfiguration
            {
                ChunkCount = 8,
                ParallelDownload = true
            });

            var path = Path.Combine(DownloadPath, FileName + ".nalai!");
            if (!File.Exists(path))
            {
                return DownloadStatus.Failed;
            }

            var packageJson = File.ReadAllText(path);

            downloader.Package = JsonConvert.DeserializeObject<DownloadPackage>(packageJson);
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            downloader.DownloadStarted += OnDownloadStarted;
            downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
            downloader.DownloadFileCompleted += OnDownloadFileCompleted;

            // 不知道为什么加这个才能恢复下载，但是会出现进程占用的情况
            downloader.CancelAsync();
            downloader.Pause();
            

            Downloader = downloader;
            
            // Downloader?.CancelAsync();
        }

        if (Downloader is { Status: DownloadStatus.Completed or DownloadStatus.Failed })
        {
            SqlService.InsertOrUpdate(this);
            return Downloader.Status;
        }

        if (Downloader is { Status: DownloadStatus.Stopped })
        {
            Downloader.DownloadFileTaskAsync(Downloader.Package);
            Downloader.Resume();
            SqlService.InsertOrUpdate(this);
            Status = DownloadStatus.Running;
            return DownloadStatus.Running;
        }

        if (Downloader is { IsPaused: true })
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
            Downloader?.Pause();
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
        
        Package = Downloader?.Package;
        DownloaderJson = JsonConvert.SerializeObject(Downloader);

        SqlService.InsertOrUpdate(this);

        return Downloader?.Status ?? DownloadStatus.None;
    }


    private void UpdateStatus()
    {
        if (Downloader != null) Status = Downloader.Status;
        // SqlService.InsertOrUpdate(this);
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

        // Console.WriteLine($"{FileName} Progress: {progress}% Speed: {speed}KB/s, Remaining: {remaining} bytes");

        TotalBytesToReceive = e.TotalBytesToReceive;

        UpdateStatus();

        // Debug.WriteLine($"Chunks: {chunks}, Progress: {progress}, Speed: {speed}KB/s, Remaining: {remaining} bytes");
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
        else if (e.Error is IOException)
        {
            // TODO: 应添加重试次数限制
            NalaiDownService.StopTask(this);
            PauseOrResume();
        }
        else
        {
            NalaiMsgBox.Show(e.Error.Message, "Download failed!");
        }

        SqlService.InsertOrUpdate(this);
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