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
            foreach (var task in tasks)
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
    }
}