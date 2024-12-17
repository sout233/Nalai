using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.Views.Windows;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Windows;

public partial class NewTaskWindowViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();

    [ObservableProperty] private string _url;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private string _savePath;
    [ObservableProperty] private bool _dialogResult;
    [ObservableProperty] private string _runningState;
    [ObservableProperty] private bool _downloadable;
    
    private string _lastPath = "";


    public NewTaskWindowViewModel(string url, string savePath)
    {
        Url = url;
        SavePath = string.IsNullOrEmpty(savePath)? GetFolderDefault.GetDownloadPath() : savePath;
        _runningState = "Checking...";

        var flyout = new Flyout
        {
            Placement = PlacementMode.MousePoint
        };
        flyout.Show();
        RunningStateChecker.StatusChanged += UpdateCoreState;
        RunningStateChecker.Check();
        UpdateCoreState(null,null);
    }


    private void UpdateCoreState(object? sender, object? e)
    {
        RunningState = RunningStateFormatter.Format(RunningStateChecker.Status);
        Downloadable = RunningStateChecker.Status == HealthStatus.Running;
        Console.WriteLine($"Running state: {RunningState}");
    }

    [RelayCommand]
    private void TestTaskKill()
    {
        const string processName = "nalai_core";

        // 获取所有匹配的进程
        var processes = Process.GetProcessesByName(processName);

        foreach (var proc in processes)
        {
            try
            {
                // 结束进程
                proc.Kill();
                Console.WriteLine($"进程 {processName} 已结束.");
            }
            catch (Exception ex)
            {
                // 如果无法结束进程，输出错误信息
                Console.WriteLine($"无法结束进程 {processName}: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void PlaceToDefault()
    {
        _lastPath = SavePath;
        SavePath = GetFolderDefault.GetDownloadPath();
    }

    [RelayCommand]
    private void PlaceToDefaultUndo()
    {
        if (_lastPath != "") SavePath = _lastPath;
    }

    [RelayCommand]
    private Task OpenFolder()
    {
        Microsoft.Win32.OpenFolderDialog dialog = new()
        {
            Multiselect = false,
            Title = "Nalai - 选择下载文件夹"
        };
        var result = dialog.ShowDialog();

        if (result == true)
        {
            SavePath = dialog.FolderName;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddTask()
    {
        try
        {
            var fileName = await UrlHelper.GetFileName(Url);
            var task = await NalaiDownService.NewTask(Url, SavePath, fileName, Headers);

            var vm = new DownloadingWindowViewModel(task);
            var window = new DownloadingWindow(vm, Url, task);
            window.Show();

            task.BindWindows.Add(window);

            Window?.Close();
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
        }
    }


    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }



    public void OnWindowClosing()
    {
        RunningStateChecker.StatusChanged -= UpdateCoreState;
        //Window.Close();
    }
}