using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Wpf.Ui.Controls;
using DownloadProgressChangedEventArgs = Nalai.Models.DownloadProgressChangedEventArgs;

namespace Nalai.ViewModels.Windows;

public partial class
    DownloadingWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "Downloading...";
    [ObservableProperty] private string? _fileName = "Unknown";
    [ObservableProperty] private double _progressValue = 0;
    [ObservableProperty] private string _progressText = "0%";
    [ObservableProperty] private string _downloadSpeed = "0 B/s";
    [ObservableProperty] private string _fileSize = "Unknown";
    [ObservableProperty] private string _remainingTime = "Unknown";
    [ObservableProperty] private string _url = "Unknown";
    [ObservableProperty] private string _showMoreBtnContent = "More";
    [ObservableProperty] private string _pauseOrResumeBtnContent = "Pause";
    [ObservableProperty] private ObservableCollection<ChunkProgressData> _chunkProgressBars = [];
    [ObservableProperty] private Visibility _showMoreVisibility = Visibility.Collapsed;
    [ObservableProperty] private SymbolIcon _showMoreBtnIcon = new() { Symbol = SymbolRegular.ChevronDown24 };
    [ObservableProperty] private SymbolIcon _pauseOrResumeBtnIcon = new() { Symbol = SymbolRegular.Pause24 };

    public DownloadingWindowViewModel(CoreTask thisViewTask)
    {
        ThisViewTask = thisViewTask;
    }

    public CoreTask ThisViewTask { get; set; }
    public Window BasedWindow { get; set; }

    [RelayCommand]
    private void CopyUrl()
    {
        Clipboard.SetText(Url);
    }

    [RelayCommand]
    private async Task PauseOrResumeDownload()
    {
        var isRunning = await ThisViewTask.StartOrCancel();
        Console.WriteLine(isRunning);
        if (isRunning)
        {
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.Pause24 };
            PauseOrResumeBtnContent = "Pause";
            ApplicationTitle = "Downloading: " + FileName;
        }
        else
        {
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.CaretRight24 };
            PauseOrResumeBtnContent = "Resume";
            ApplicationTitle = "Paused: " + FileName;
        }
    }


    [RelayCommand]
    private async Task CancelDownload()
    {
        await ThisViewTask.CancelAsync();
        BasedWindow.Close();
    }

    public void AddOrUpdateChunkProgressBars(string id, float value)
    {
        var chunk = ChunkProgressBars.FirstOrDefault(x => x.Id == id);
        if (chunk == null)
        {
            chunk = new ChunkProgressData { Id = id, Value = value };
            ChunkProgressBars.Add(chunk);
        }
        else
        {
            ChunkProgressBars[ChunkProgressBars.IndexOf(chunk)] = new ChunkProgressData { Id = id, Value = value };
        }
    }

    public void OnChunkDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        // var id = e.ProgressId;
        var progress = e.ProgressPercentage;

        // TODO: Fix this (chuck进度条显示会卡卡的)
        // Application.Current.Dispatcher.Invoke((Action)(() =>
        // {
        //     AddOrUpdateChunkProgressBars(id, (float)progress);
        // }));
    }


    public void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var totalFileSize = ByteSizeFormatter.FormatSize(e.TotalBytesToReceive);
        var receivedFileSize = ByteSizeFormatter.FormatSize(e.BytesReceived);
        var remainingTime =
            TimeFormatter.CalculateRemainingTime(e.BytesReceived, e.TotalBytesToReceive,
                (long)e.BytesPerSecondSpeed);
        
        ProgressValue = e.ProgressPercentage;
        ProgressText = e.ProgressPercentage.ToString("0.00") + "%";
        DownloadSpeed = ByteSizeFormatter.FormatSize((long)e.BytesPerSecondSpeed) + "/s";
        
        RemainingTime = $"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
        Url = ThisViewTask.Url;
        FileSize = $"{receivedFileSize} / {totalFileSize}";
    }

    public void OnDownloadStatusChanged(object? sender, NalaiCoreInfo e)
    {
        FileName = e.FileName;
        ApplicationTitle = "Downloading: " + FileName;
        Url = e.Url;
    }
}