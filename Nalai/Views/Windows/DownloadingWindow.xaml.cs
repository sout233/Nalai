using Downloader;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadingWindow : FluentWindow
{
    public DownloadingWindowViewModel ViewModel { get; }
    public static string Url { get; set; }

    const string DOWNLOAD_URL =
        "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

    public static DownloadingWindow CreateWindow(string url)
    {
        var viewModel = new DownloadingWindowViewModel();
        return new DownloadingWindow(viewModel, url);
    }

    public DownloadingWindow(DownloadingWindowViewModel viewModel, string url)
    {
        ViewModel = viewModel;
        DataContext = this;
        Url = url;
        InitializeComponent();
    }

    private void DownloadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
       var task =  NalaiDownService.GetTaskByUrl(Url);
       var fileName = task.FileName;
        ViewModel.FileName = fileName;
        ViewModel.ApplicationTitle = "Downloading: " + fileName;
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        ViewModel.Progress = e.ProgressPercentage;
    }
}