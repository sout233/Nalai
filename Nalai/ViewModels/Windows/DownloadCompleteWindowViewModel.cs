using System.Diagnostics;
using System.IO;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Resources;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class DownloadCompleteWindowViewModel : ObservableObject
{
    public DownloadCompleteWindow? BindWindow { get; set; }

    [ObservableProperty] private string _applicationTitle = I18NExtension.Translate(LangKeys.dc_downloadComplete);
    [ObservableProperty] private string _fileName = "Unknown";
    [ObservableProperty] private string _downloadPath = "Unknown";

    public DownloadCompleteWindowViewModel(CoreTask task)
    {
        FileName = task.FileName;
        DownloadPath = task.SaveDir;
        ApplicationTitle = $"{I18NExtension.Translate(LangKeys.dc_downloadComplete)} - {task.FileName}";
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
            // TODO: 该文件下载后找不到，可能是core存在问题：https://mirrors.tuna.tsinghua.edu.cn/debian/dists/Debian10.13/ChangeLog
            NalaiMsgBox.Show($"{I18NExtension.Translate(LangKeys.msg_content_fileNotExist)}:\n{filePath}\n{I18NExtension.Translate(LangKeys.msg_content_delOrMove)}?", I18NExtension.Translate(LangKeys.msg_title_error));
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
            NalaiMsgBox.Show($"{I18NExtension.Translate(LangKeys.msg_content_fileNotExist)}:\n{filePath}\n{I18NExtension.Translate(LangKeys.msg_content_delOrMove)}?", I18NExtension.Translate(LangKeys.msg_title_error));
        }
        
        BindWindow?.Close();
    }
}