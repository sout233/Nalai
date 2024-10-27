using System.Collections.ObjectModel;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Wpf.Ui.Controls;

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

    public DownloadTask ThisViewTask { get; set; }
    public Window BasedWindow { get; set; }

    [RelayCommand]
    private void CopyUrl()
    {
        Clipboard.SetText(Url);
    }

    [RelayCommand]
    private void PauseOrResumeDownload()
    {
        if (ThisViewTask.Downloader.IsPaused)
        {
            ThisViewTask.Downloader.Resume();
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.Pause24 };
            PauseOrResumeBtnContent = "Pause";
        }
        else
        {
            ThisViewTask.Downloader.Pause();
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.CaretRight24 };
            PauseOrResumeBtnContent = "Resume";
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        ThisViewTask.Downloader.CancelAsync();
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
        var id = e.ProgressId;
        var progress = e.ProgressPercentage;

        // TODO: Fix this
        // Application.Current.Dispatcher.Invoke((Action)(() =>
        // {
        //     AddOrUpdateChunkProgressBars(id, (float)progress);
        // }));
    }

    public void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var chunks = e.ActiveChunks;
        var progress = e.ProgressPercentage;
        var speed = ByteSizeFormatter.FormatSize((long)e.BytesPerSecondSpeed);
        var remaining = e.TotalBytesToReceive - e.ReceivedBytesSize;
        var totalFileSize = ByteSizeFormatter.FormatSize(e.TotalBytesToReceive);
        var receivedFileSize = ByteSizeFormatter.FormatSize(e.ReceivedBytesSize);
        var remainingTime =
            TimeFormatter.CalculateRemainingTime(e.ReceivedBytesSize, e.TotalBytesToReceive,
                (long)e.BytesPerSecondSpeed);

        // ViewModel.SetProgress(progress);
        // ViewModel.SetProgressText(progress.ToString("0.00") + "%");
        // ViewModel.SetDownloadSpeed(speed + "/s");
        // ViewModel.SetFileSize($"{receivedFileSize} / {totalFileSize}");
        // ViewModel.SetRemainingTime($"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s");
        // ViewModel.Url = ThisWindowTask.Url;

        ProgressValue = progress;
        ProgressText = progress.ToString("0.00") + "%";
        DownloadSpeed = speed + "/s";
        FileSize = $"{receivedFileSize} / {totalFileSize}";
        RemainingTime = $"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
        Url = ThisViewTask.Url;
    }

    public void SetProgress(double value)
    {
        ProgressValue = value;
    }

    public void SetFileName(string? value)
    {
        FileName = value;
    }

    public void SetRemainingTime(string value)
    {
        RemainingTime = value;
    }

    public void SetFileSize(string value)
    {
        FileSize = value;
    }

    public void SetDownloadSpeed(string value)
    {
        DownloadSpeed = value;
    }

    public void SetProgressText(string value)
    {
        ProgressText = value;
    }
}