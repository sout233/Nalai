﻿using System.Collections.ObjectModel;
using Downloader;
using Nalai.CoreConnector.Models;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private string _pauseOrResumeText = "暂停";
        [ObservableProperty] private SymbolIcon _pauseOrResumeIcon = new() { Symbol = SymbolRegular.Pause24 };

        [ObservableProperty] private ObservableCollection<CoreTask> _downloadViewItems;

        public DashboardViewModel()
        {
            // NalaiDownService.GlobalDownloadTasks = SqlService.ReadAll();
            UpdateDownloadCollection();

            foreach (var task in NalaiDownService.GlobalDownloadTasks)
            {
                if (task != null) task.StatusChanged += OnDownloadStatusChanged;
            }

            // SqlService.OnInsertOrUpdate += OnSqlInsertOrUpdate;
        }

        private void OnSqlInsertOrUpdate(object? sender, CoreTask e)
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

        private ObservableCollection<CoreTask> GenerateDownloadCollection()
        {
            var tasks = NalaiDownService.GlobalDownloadTasks;
            var taskCollection = new ObservableCollection<CoreTask>();
            foreach (var task in tasks) // TODO: 可能会导致右键菜单无法正常显示
            {
                if (task != null)
                {
                    taskCollection.Add(task);
                }
            }

            return taskCollection;
        }

        private void UpdateDownloadCollection()
        {
            DownloadViewItems = GenerateDownloadCollection();
        }

        public void OnDownloadStatusChanged(object? sender, GetStatusResult getStatusResult)
        {
            UpdateDownloadCollection();
        }

        private void UpdateRightClickMenu(DownloadStatus status)
        {
            PauseOrResumeText = status switch
            {
                DownloadStatus.Running => "暂停",
                DownloadStatus.Paused => "继续",
                DownloadStatus.Completed => "重新下载",
                DownloadStatus.Failed => "重新下载",
                DownloadStatus.Stopped => "重新下载",
                _ => PauseOrResumeText
            };

            PauseOrResumeIcon = status switch
            {
                DownloadStatus.Running => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatus.Paused => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatus.Completed => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatus.Failed => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatus.Stopped => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                _ => PauseOrResumeIcon
            };
        }

        [RelayCommand]
        private async Task OnPauseOrResume(object? parameter)
        {
            if (parameter is not ListView item) return;
            if (item.SelectedItem is not CoreTask task) return;

            await task.StopAsync();
            
            UpdateRightClickMenu(status);
        }

        [RelayCommand]
        private void OnRemove(object? parameter)
        {
            if (parameter is not ListView item) return;
            if (item.SelectedItem is not CoreTask task) return;

            NalaiDownService.RemoveTask(task);
            UpdateDownloadCollection();
        }

        [RelayCommand]
        private void OnCancel(object? parameter)
        {
            if (parameter is not ListView item) return;
            if (item.SelectedItem is not CoreTask task) return;

            NalaiDownService.StopTask(task);
            UpdateRightClickMenu(task.Status);
        }
    }
}