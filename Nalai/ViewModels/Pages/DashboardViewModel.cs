using System.Collections.ObjectModel;
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
        [ObservableProperty] private bool _isPauseOrResumeEnabled;
        [ObservableProperty] private SymbolIcon _pauseOrResumeButtonIcon = new() { Symbol = SymbolRegular.Pause24 };

        [ObservableProperty] private ObservableCollection<CoreTask> _downloadViewItems;

        public DashboardViewModel()
        {
            // NalaiDownService.GlobalDownloadTasks = SqlService.ReadAll();
            UpdateDownloadCollection();

            foreach (var task in NalaiDownService.GlobalDownloadTasks)
            {
                if (task != null) task.StatusChanged += OnDownloadStatusChanged;
            }

            CoreTask.GlobalTaskChanged += OnGlobalTaskChanged;
        }

        public void SetPauseOrResumeButtonEnabled(bool isEnabled)
        {
            IsPauseOrResumeEnabled = isEnabled;
        }

        private void OnGlobalTaskChanged(object? sender, CoreTask e)
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

        public void OnDownloadStatusChanged(object? sender, NalaiCoreInfo nalaiCoreInfo)
        {
            UpdateDownloadCollection();
        }

        public void UpdatePauseOrResumeElement(DownloadStatus status)
        {
            PauseOrResumeText = status switch
            {
                DownloadStatus.Running => "暂停",
                DownloadStatus.Pending => "暂停",
                DownloadStatus.NoStart => "继续",
                DownloadStatus.Cancelled => "继续",
                DownloadStatus.Finished => "重新下载",
                DownloadStatus.Error => "重新下载",
                _ => PauseOrResumeText
            };

            PauseOrResumeIcon = status switch
            {
                DownloadStatus.Running => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatus.Pending => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatus.NoStart => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatus.Cancelled => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatus.Finished => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatus.Error => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                _ => PauseOrResumeIcon
            };

            PauseOrResumeButtonIcon = status switch
            {
                DownloadStatus.Running => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatus.Pending => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatus.NoStart => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatus.Cancelled => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatus.Finished => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatus.Error => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                _ => PauseOrResumeButtonIcon
            };
        }

        [RelayCommand]
        private async Task OnPauseOrResume(object? parameter)
        {
            if (parameter is not ListView item) return;

            foreach (var coreTask in item.SelectedItems.OfType<CoreTask>())
            {
                var result = await coreTask.StartOrCancelAsync();
                UpdatePauseOrResumeElement(result ? DownloadStatus.Running : coreTask.Status);
            }

            UpdateDownloadCollection();
        }

        [RelayCommand]
        private async Task OnRemove(object? parameter)
        {
            if (parameter is not ListView item) return;

            foreach (var coreTask in item.SelectedItems.OfType<CoreTask>())
            {
                await coreTask.DeleteAsync();
                UpdatePauseOrResumeElement(coreTask.Status);
            }

            UpdateDownloadCollection();
        }

        [RelayCommand]
        private async Task OnCancel(object? parameter)
        {
            if (parameter is not ListView item) return;

            foreach (var coreTask in item.SelectedItems.OfType<CoreTask>())
            {
                await coreTask.CancelAsync();
                UpdatePauseOrResumeElement(coreTask.Status);
            }

            UpdateDownloadCollection();
        }

        [RelayCommand]
        private void OnShowDetails(object? parameter)
        {
            if (parameter is not ListView item) return;
            if (item.SelectedItem is not CoreTask task) return;

            var window = new DetailsWindow(task);
            window.Show();
        }
    }
}