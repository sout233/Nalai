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
        return new DownloadingWindow(viewModel, url,task);
    }

    public DownloadingWindow(DownloadingWindowViewModel viewModel, string url,DownloadTask task)
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
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        ViewModel.SetProgress(e.ProgressPercentage);
        Console.WriteLine(e.ProgressPercentage);
    }
}