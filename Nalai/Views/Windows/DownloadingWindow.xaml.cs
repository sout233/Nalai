using Downloader;
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
        InitializeComponent();
    }

    private async void DownloadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine(ViewModel.FileName);
        NalaiDownService.NewTask(DOWNLOAD_URL, "1.zip", Environment.CurrentDirectory);
    }
    
}