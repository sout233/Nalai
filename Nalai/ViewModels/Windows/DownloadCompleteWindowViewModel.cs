﻿using System.Diagnostics;
using System.IO;
using Nalai.Helpers;
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
        var filePath = Path.Combine(DownloadPath, FileName);

        if (File.Exists(filePath))
        {
            Process.Start("explorer.exe", $"/select,{filePath}");
        }
        else
        {
            NalaiMsgBox.Show($"文件不存在:\n{filePath}\n是否已经移动或删除？", "错误");
        }
        
        BindWindow?.Close();
    }

    [RelayCommand]
    private void OnOpenFile()
    {
        var filePath = Path.Combine(DownloadPath, FileName);

        if (File.Exists(filePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        else
        {
            NalaiMsgBox.Show($"文件不存在:\n{filePath}\n是否已经移动或删除？", "错误");
        }
        
        BindWindow?.Close();
    }
}