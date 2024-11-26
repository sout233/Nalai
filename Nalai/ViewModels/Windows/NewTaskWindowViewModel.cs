using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Pages;
using Nalai.Views.Windows;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Windows;

public partial class NewTaskWindowViewModel : ObservableObject
{
    public Window Window { get; set; }

    [ObservableProperty] private string _url;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private string _savePath;
    [ObservableProperty] private bool _dialogResult;
    [ObservableProperty] private string _runningState;
    private string _lastPath = "";
    

    public DashboardViewModel Dashboard { get; set; }
    HealthChecker Hc = new();
    public NewTaskWindowViewModel(string url = null, string savePath = null)
    {
        //Console.WriteLine(defaultPath);
        Url = url;
        SavePath = savePath;
        //NalaiMsgBox.Show($"默认路径为： {DefaultPath}");
        var flyout = new Flyout();
        flyout.Placement = PlacementMode.MousePoint;
        flyout.Show();
        Console.WriteLine(flyout.IsOpen);
        
        Hc.Start();
        Console.WriteLine(RunningState);
        Hc.StatusChanged += UpdateCoreState;
    }
    


    private void UpdateCoreState(object sender,EventArgs e)
    {
        RunningState = Hc.Status switch
        {
            HealthStatus.Running => "Success",
            HealthStatus.Unknown => "Critical",
            _ => ""
        };
    }

    [RelayCommand]
    private void TestTaskKill()
    {
        string processName = "nalai_core";

        // 获取所有匹配的进程
        Process[] processes = Process.GetProcessesByName(processName);

        foreach (Process proc in processes)
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
        var fileName = await UrlHelper.GetFileName(Url);
        var task = await NalaiDownService.NewTask(Url, SavePath, fileName);

        var vm = new DownloadingWindowViewModel(task);
        var window = new DownloadingWindow(vm, Url, task);
        window.Show();

        task.BindWindows.Add(window);

        WindowClose();
    }


    [RelayCommand]
    private void Cancel()
    {
        WindowClose();
    }

    private void WindowClose(bool c=true)
    {
        Hc.Stop();
        if(c)Window.Close();
    }

    public void OnWindowClosing()
    {
        WindowClose(false);
    }
}