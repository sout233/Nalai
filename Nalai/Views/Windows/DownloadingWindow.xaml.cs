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

        ViewModel.ChunkProgressBars.CollectionChanged += (sender, args) =>
        {
//TODO: Change UniformGris Column here
        };
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
        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
        {
        ViewModel.AddOrUpdateChunkProgressBars(id, (float)progress *100);
        }));
        
        // int id = 0;
        //
        // try
        // {
        //     id = int.Parse(e.ProgressId);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Failed to parse progress ID: {ex.Message}");
        //     return;
        // }
        //
        // var chunkValues = ViewModel.ChunkValues;
        // bool updated = false;
        //
        // foreach (var chunkValue in chunkValues)
        // {
        //     if (chunkValue.Index == id)
        //     {
        //         if (chunkValue.Progress != e.ProgressPercentage)
        //         {
        //             chunkValue.Progress = (float)e.ProgressPercentage;
        //             updated = true;
        //         }
        //         break;
        //     }
        // }
        //
        // if (updated)
        // {
        //     ViewModel.ChunkValues = chunkValues;
        // }
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
        // Console.WriteLine(e.ProgressPercentage);
    }
}