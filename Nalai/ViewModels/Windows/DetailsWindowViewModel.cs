﻿using Nalai.Helpers;
using Nalai.Models;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class DetailsWindowViewModel : ObservableObject
{
    public DetailsWindow BindWindow { get; set; }
    [ObservableProperty] private string _applicationTitle = "Details: Unknown";
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _savePath;
    [ObservableProperty] private string _fileSize;
    [ObservableProperty] private string _status;
    [ObservableProperty] private string _downloadedSize;
    [ObservableProperty] private string _totalSize;
    [ObservableProperty] private string _speed;
    [ObservableProperty] private string _url;

    public DetailsWindowViewModel(CoreTask task)
    {
        ApplicationTitle = "Details: " + task.FileName;
        ProgressText = task.RealtimeStatusText;
        FileName = task.FileName;
        Id = task.Id ?? string.Empty;
        SavePath = task.SavePath;
        FileSize = task.FileSizeText;
        Status = task.InfoResult.Status.ToString();
        DownloadedSize = ByteSizeFormatter.FormatSize(task.InfoResult.DownloadedBytes);
        TotalSize = ByteSizeFormatter.FormatSize(task.InfoResult.TotalBytes);
        Speed = ByteSizeFormatter.FormatSize(task.InfoResult.BytesPerSecondSpeed)+"/s";
        Url = task.Url;
    }
    
    [RelayCommand]
    private void CloseWindow()
    {
        // TODO: Implement closing window for MVVM
        // Messenger.Default.Send(new CloseWindowMessage());
        BindWindow.Close();
    }
}