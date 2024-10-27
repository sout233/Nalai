using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Pages;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class NewTaskWindowViewModel : ObservableObject
{
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
    private async Task AddTask()
    {
        var fileName = GetUrlInfo.GetFileName(Url);
        
        var task = await NalaiDownService.NewTask(Url, fileName, Environment.CurrentDirectory);
        
        task.StatusChanged +=Dashboard.OnDownloadStatusChanged;
        
        var vm = new DownloadingWindowViewModel();
        var window = new DownloadingWindow(vm, Url, task);
        window.Show();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}