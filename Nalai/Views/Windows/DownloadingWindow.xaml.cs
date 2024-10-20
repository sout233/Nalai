using Downloader;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadingWindow : FluentWindow
{
    public DownloadingWindowViewModel ViewModel { get; }

    const string DOWNLOAD_URL =
    "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

    public static DownloadingWindow CreateWindow()
    {
        var viewModel = new DownloadingWindowViewModel();
        return new DownloadingWindow(viewModel);
    }

    public DownloadingWindow(DownloadingWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void DownloadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine(ViewModel.FileName);
        var fileName = GetUrlInfo.GetFileName(DOWNLOAD_URL);
        ViewModel.FileName = fileName;
        ViewModel.ApplicationTitle = "Downloading: " + fileName;
       var task =  NalaiDownService.NewTask(DOWNLOAD_URL, fileName, Environment.CurrentDirectory).Result;
       task.Downloader.DownloadProgressChanged+=OnDownloadProgressChanged;
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        ViewModel.Progress = e.ProgressPercentage;
    }
}