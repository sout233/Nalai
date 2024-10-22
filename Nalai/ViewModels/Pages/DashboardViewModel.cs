using System.Collections.ObjectModel;
using System.Diagnostics;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private const string DownloadUrl =
            "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

        public DashboardViewModel()
        {
            UpdateDownloadCollection();
        }
        
        [RelayCommand]
        private async Task OnNewTask()
        {
            var fileName = GetUrlInfo.GetFileName(DownloadUrl);

            var task = await NalaiDownService.NewTask(DownloadUrl, fileName, Environment.CurrentDirectory);

            UpdateDownloadCollection();
            task.StatusChanged += OnDownloadStatusChanged;
            
            var vm = new DownloadingWindowViewModel();
            var window = new DownloadingWindow(vm, DownloadUrl, task);
            window.Show();
        }
        
        [ObservableProperty]
        private ObservableCollection<DownloadTask> _downloadViewItems = GenerateDownloadCollection();

        private static ObservableCollection<DownloadTask> GenerateDownloadCollection()
        {
            var tasks = NalaiDownService.GlobalDownloadTasks;
            var taskCollection = new ObservableCollection<DownloadTask>();
            foreach (var task in tasks)
            {
                if (task != null) taskCollection.Add(task);
                Console.WriteLine(task.Progress);
            }
       
            return taskCollection;
        }

        private void UpdateDownloadCollection()
        {
            DownloadViewItems = GenerateDownloadCollection();
        }

        private void OnDownloadStatusChanged(object? sender, EventArgs eventArgs)
        {
            UpdateDownloadCollection();
        }
    }
}