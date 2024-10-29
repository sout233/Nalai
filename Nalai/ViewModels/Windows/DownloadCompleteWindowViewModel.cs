using System.Diagnostics;
using System.IO;
using Nalai.Models;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class DownloadCompleteWindowViewModel : ObservableObject
{
    public DownloadCompleteWindow? BindWindow { get; set; }

    [ObservableProperty] private string _applicationTitle = "Download Complete";
    [ObservableProperty] private string _fileName = "Unknown";
    [ObservableProperty] private string _downloadPath = "Unknown";

    public DownloadCompleteWindowViewModel(DownloadTask task)
    {
        FileName = task.FileName;
        DownloadPath = task.DownloadPath;
        ApplicationTitle = $"Download Complete: {task.FileName}";
    }

    [RelayCommand]
    private void OnCloseWindow()
    {
        BindWindow?.Close();
    }

    [RelayCommand]
    private void OnOpenFolder()
    {
        Process.Start("explorer.exe", $"/select,{DownloadPath}");
    }

    [RelayCommand]
    private void OnOpenFile()
    {
        Process.Start("explorer.exe", $"/select,{Path.Combine(DownloadPath, FileName)}");
    }
}