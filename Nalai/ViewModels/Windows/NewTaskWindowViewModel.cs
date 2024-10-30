using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Pages;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class NewTaskWindowViewModel : ObservableObject
{
    public Window Window { get; set; }

    [ObservableProperty] private string _url;
    [ObservableProperty] private string _savePath;
    [ObservableProperty] private bool _dialogResult;

    public DashboardViewModel Dashboard { get; set; }

    public NewTaskWindowViewModel(string url = "", string savePath = "")
    {
        this.Url = url;
        this.SavePath = savePath;
    }

    [RelayCommand]
    private Task OpenFolder()
    {
        Microsoft.Win32.OpenFolderDialog dialog = new();
        dialog.Multiselect = false;
        dialog.Title = "Nalai:选择下载文件夹";
// Show open folder dialog box
        bool? result = await Application.Current.Dispatcher.InvokeAsync(() => 
        {
            return (bool?)dialog.ShowDialog();
        }).Task;
// Process open folder dialog box results
        Microsoft.Win32.OpenFolderDialog dialog = new()
        {
            Multiselect = false,
            Title = "Nalai - 选择下载文件夹"
        };
        var result = dialog.ShowDialog();
        
        if (result == true)
        {
            this.SavePath = dialog.FolderName;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddTask()
    {
        var fileName = await UrlHelper.GetFileName(Url);
        var task = await NalaiDownService.NewTask(Url, fileName, SavePath);

        task.StatusChanged += Dashboard.OnDownloadStatusChanged;

        var vm = new DownloadingWindowViewModel(task);
        var window = new DownloadingWindow(vm, Url, task);
        window.Show();
        
        task.BindWindows.Add(window);

        Window.Close();
    }
    

    [RelayCommand]
    private void Cancel()
    {
        Window.Close();
    }
}