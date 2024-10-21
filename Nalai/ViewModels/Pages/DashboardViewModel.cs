using System.Diagnostics;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private int _counter = 0;

        const string DOWNLOAD_URL =
            "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

        [RelayCommand]
        private async Task OnCounterIncrement()
        {
            Counter++;

            var fileName = GetUrlInfo.GetFileName(DOWNLOAD_URL);

            var task = await NalaiDownService.NewTask(DOWNLOAD_URL, fileName, Environment.CurrentDirectory);

            var vm = new DownloadingWindowViewModel();
            var window = new DownloadingWindow(vm, DOWNLOAD_URL, task);
            window.Show();
        }
    }
}