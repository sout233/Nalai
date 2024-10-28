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
        [ObservableProperty] private string _pauseOrResumeText = "暂停";
        
        public DashboardViewModel()
        {
            UpdateDownloadCollection();
        }

        [RelayCommand]
        private Task OnNewTask()
        {
            var newTaskWindowViewModel = new NewTaskWindowViewModel();
            var window = new NewTaskWindow(newTaskWindowViewModel);
            newTaskWindowViewModel.Window = window;
            newTaskWindowViewModel.Dashboard = this;
            window.Show();

            UpdateDownloadCollection();

            return Task.CompletedTask;
        }

        [ObservableProperty]
        private ObservableCollection<DownloadTask> _downloadViewItems = GenerateDownloadCollection();

        private static ObservableCollection<DownloadTask> GenerateDownloadCollection()
        {
            var tasks = NalaiDownService.GlobalDownloadTasks;
            var taskCollection = new ObservableCollection<DownloadTask>();
            foreach (var task in tasks)            // TODO: 可能会导致右键菜单无法正常显示

            {
                if (task != null) taskCollection.Add(task);
            }

            return taskCollection;
        }

        private void UpdateDownloadCollection()
        {
            DownloadViewItems = GenerateDownloadCollection();
        }

        public void OnDownloadStatusChanged(object? sender, EventArgs eventArgs)
        {
            UpdateDownloadCollection();
        }

        [RelayCommand]
        private void OnPause(object? parameter)
        {
            if (parameter is not Wpf.Ui.Controls.ListView item) return;
            if (item.SelectedItem is not DownloadTask task) return;
            
            var result = task.PauseOrResume();
            PauseOrResumeText = result switch
            {
                DownloadStatus.Running => "暂停",
                DownloadStatus.Paused => "继续",
                DownloadStatus.Completed => "重新下载",     // TODO: 实现重新下载
                DownloadStatus.Failed => "重新下载",
                DownloadStatus.Stopped => "重新下载",
                _ => PauseOrResumeText
            };
        }

        [RelayCommand]
        private void OnDelete(object? parameter)
        {
            if (parameter is not Wpf.Ui.Controls.ListView item) return;
            if (item.SelectedItem is not DownloadTask task) return;

            NalaiDownService.RemoveTask(task);
            UpdateDownloadCollection();
        }
    }
}