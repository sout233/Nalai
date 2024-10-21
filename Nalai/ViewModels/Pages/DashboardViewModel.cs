using System.Diagnostics;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _counter = 0;
        const string DOWNLOAD_URL =
            "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

        [RelayCommand]
        private async void OnCounterIncrement()
        {
            Counter++;
            
            var fileName = GetUrlInfo.GetFileName(DOWNLOAD_URL);

            Console.WriteLine("Downloading " + fileName);
            var task = NalaiDownService.NewTask(DOWNLOAD_URL, fileName, Environment.CurrentDirectory).Result;
            // task.Downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            Console.WriteLine("OPK");
            DownloadingWindowViewModel vm = new DownloadingWindowViewModel();
            var window = new DownloadingWindow(vm,DOWNLOAD_URL);
            window.Show();
        }
    }
}
