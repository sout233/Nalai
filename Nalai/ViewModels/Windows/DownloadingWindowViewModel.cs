using System.Collections.ObjectModel;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Resources;
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
    [ObservableProperty] private string _maxSpeedText = "0 B/s";
    [ObservableProperty] private string _fileSize = "Unknown";
    [ObservableProperty] private string _remainingTime = "Unknown";
    [ObservableProperty] private string _url = "Unknown";
    [ObservableProperty] private string _showMoreBtnContent = "More";
    [ObservableProperty] private string _pauseOrResumeBtnContent = "Pause";
    [ObservableProperty] private ObservableCollection<ChunksItem> _chunkProgressBars = [];
    [ObservableProperty] private Visibility _showMoreVisibility = Visibility.Collapsed;
    [ObservableProperty] private SymbolIcon _showMoreBtnIcon = new() { Symbol = SymbolRegular.ChevronDown24 };
    [ObservableProperty] private SymbolIcon _pauseOrResumeBtnIcon = new() { Symbol = SymbolRegular.Pause24 };
    [ObservableProperty] private ObservableCollection<ExtendedChunkItem> _chunksCollection = [];

    private long _maxSpeed = 0;

    public DownloadingWindowViewModel(NalaiCoreInfoExtended thisViewTask)
    {
        ThisViewTask = thisViewTask;
    }
    
    
    public NalaiCoreInfoExtended ThisViewTask { get; set; }
    public Window BasedWindow { get; set; }

    [RelayCommand]
    private void CopyUrl()
    {
        Clipboard.SetText(Url);
    }

    [RelayCommand]
    private async Task PauseOrResumeDownload()
    {
        // var isRunning = await ThisViewTask.StartOrCancelAsync();
        var isRunning = await ConnectorHelper.PauseOrResumeTaskById(ThisViewTask.Id);
        Console.WriteLine(isRunning);
        if (isRunning)
        {
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.Pause24 };
            PauseOrResumeBtnContent = I18NHelper.GetTranslation(LangKeys.Button_Pause);
            ApplicationTitle = I18NExtension.Translate(LangKeys.DownloadingWindow_Downloading) + ": " + FileName;
        }
        else
        {
            PauseOrResumeBtnIcon = new SymbolIcon { Symbol = SymbolRegular.CaretRight24 };
            PauseOrResumeBtnContent = I18NHelper.GetTranslation(LangKeys.Button_Resume);
            ApplicationTitle = I18NExtension.Translate(LangKeys.DownloadingWindow_Paused) + ": " + FileName;
        }
    }


    [RelayCommand]
    private async Task CancelDownload()
    {
        // await ThisViewTask.CancelAsync();
        await ConnectorHelper.StopTaskById(ThisViewTask.Id);
        BasedWindow.Close();
    }

    public void AddOrUpdateChunkProgressBars(int id, float value)
    {
        var chunk = ChunkProgressBars.FirstOrDefault(x => x.Index == id);
        if (chunk == null)
        {
            // chunk = new ChunkProgressData { Id = id, Value = value };
            ChunkProgressBars.Add(chunk);
        }
        else
        {
            // ChunkProgressBars[ChunkProgressBars.IndexOf(chunk)] = new ChunkProgressData { Id = id, Value = value };
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
                e.BytesPerSecondSpeed);

        ProgressValue = e.ProgressPercentage;
        ProgressText = e.ProgressPercentage.ToString("0.00") + "%";
        DownloadSpeed = ByteSizeFormatter.FormatSize((long)e.BytesPerSecondSpeed) + "/s";
        if (e.BytesPerSecondSpeed > _maxSpeed)
            _maxSpeed = e.BytesPerSecondSpeed;
        MaxSpeedText = ByteSizeFormatter.FormatSize(_maxSpeed) + "/s";

        RemainingTime = $"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
        // Url = ThisViewTask.Url;
        FileSize = $"{receivedFileSize} / {totalFileSize}";
        // Task.Run(() => { ChunksCollection = GenerateChunksCollection(); });
    }

    private ObservableCollection<ExtendedChunkItem> GenerateChunksCollection(List<ChunksItem> chunks)
    {
        var chunksCollection = new ObservableCollection<ExtendedChunkItem>();
        foreach (var chunk in chunks)
        {
            chunksCollection.Add(new ExtendedChunkItem(chunk));
        }

        return chunksCollection;
    }

    public void OnDownloadStatusChanged(object? sender, NalaiCoreInfo e)
    {
        Console.WriteLine("OnDownloadStatusChanged");
        FileName = e.FileName;
        ApplicationTitle = I18NExtension.Translate(LangKeys.DownloadingWindow_Downloading) + ": " + FileName;
        Url = e.Url;
    }
    

    public void OnGlobalTaskChanged(object? sender, NalaiCoreInfo e)
    {
        var totalFileSize = ByteSizeFormatter.FormatSize(e.TotalBytes);
        var receivedFileSize = ByteSizeFormatter.FormatSize(e.DownloadedBytes);
        var remainingTime =
            TimeFormatter.CalculateRemainingTime(e.DownloadedBytes, e.TotalBytes,
                e.BytesPerSecondSpeed);

        // TODO: 改为model中的progress
        ProgressValue = (float)e.DownloadedBytes / e.TotalBytes * 100;
        ProgressText = ProgressValue.ToString("0.00") + "%";
        DownloadSpeed = ByteSizeFormatter.FormatSize(e.BytesPerSecondSpeed) + "/s";
        if (e.BytesPerSecondSpeed > _maxSpeed)
            _maxSpeed = e.BytesPerSecondSpeed;
        MaxSpeedText = ByteSizeFormatter.FormatSize(_maxSpeed) + "/s";

        RemainingTime = $"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
        Url = ThisViewTask.Url;
        FileSize = $"{receivedFileSize} / {totalFileSize}";
        Task.Run(() => { ChunksCollection = GenerateChunksCollection(e.Chunks); });
    }
}