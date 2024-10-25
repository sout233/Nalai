using System.Windows.Controls;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadingWindow : FluentWindow
{
    public DownloadingWindowViewModel ViewModel { get; }
    public string Url { get; set; }
    private DownloadTask ThisWindowTask { get; set; }

    public static DownloadingWindow CreateWindow(string url, DownloadTask task)
    {
        var viewModel = new DownloadingWindowViewModel();
        return new DownloadingWindow(viewModel, url, task);
    }

    public DownloadingWindow(DownloadingWindowViewModel viewModel, string url, DownloadTask task)
    {
        ViewModel = viewModel;
        DataContext = this;
        Url = url;
        ThisWindowTask = task;

        InitializeComponent();
    }

    private void DownloadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var task = ThisWindowTask;
        var fileName = task.FileName;

        ViewModel.FileName = fileName;
        ViewModel.ApplicationTitle = "Downloading: " + fileName;

        task.Downloader.DownloadProgressChanged += OnDownloadProgressChanged;
        task.Downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
    }

    private void OnChunkDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var id = e.ProgressId;
        var progress = e.ProgressPercentage;
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            ViewModel.AddOrUpdateChunkProgressBars(id, (float)progress);
        }));
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
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

        ViewModel.SetProgress(progress);
        ViewModel.SetProgressText(progress.ToString("0.00") + "%");
        ViewModel.SetDownloadSpeed(speed + "/s");
        ViewModel.SetFileSize($"{receivedFileSize} / {totalFileSize}");
        ViewModel.SetRemainingTime($"{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s");
    }

    private void ShowMoreBtn_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.IsShowMore = !ViewModel.IsShowMore;
        Height = ViewModel.IsShowMore ? 500 : 370;
        if (ViewModel.IsShowMore)
        {
        }
    }
}