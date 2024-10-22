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
        const string DOWNLOAD_URL =
            "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

        [RelayCommand]
        private async Task OnNewTask()
        {
            var fileName = GetUrlInfo.GetFileName(DOWNLOAD_URL);

            var task = await NalaiDownService.NewTask(DOWNLOAD_URL, fileName, Environment.CurrentDirectory);

            UpdateDownloadCollection();
            
            var vm = new DownloadingWindowViewModel();
            var window = new DownloadingWindow(vm, DOWNLOAD_URL, task);
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
            }
       
            return taskCollection;
        }

        public void UpdateDownloadCollection()
        {
            DownloadViewItems = GenerateDownloadCollection();
        }
    }
}