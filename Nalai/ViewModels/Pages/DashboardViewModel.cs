using System.Collections.ObjectModel;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
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

        [ObservableProperty] private string _searchText;

        partial void OnSearchTextChanging(string? value)
        {
            if (value != null) UpdateSearchedItems(value);
        }

        [ObservableProperty] private string _sortTypeText = "文件名";
        [ObservableProperty] private SymbolIcon _sortTypeIcon = new() { Symbol = SymbolRegular.ArrowDown24 };

        [ObservableProperty] private ObservableCollection<CoreTask> _downloadViewItems;
        private readonly ObservableCollection<CoreTask> _originalDownloadViewItems = [];

        public DashboardViewModel()
        {
            // NalaiDownService.GlobalDownloadTasks = SqlService.ReadAll();
            UpdateDownloadCollection();

            foreach (var (_, task) in NalaiDownService.GlobalDownloadTasks)
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
            var newTaskWindowViewModel = new NewTaskWindowViewModel(string.Empty, GetFolderDefault.GetDownloadPath());
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
            foreach (var (_, task) in tasks)
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

        private void OnDownloadStatusChanged(object? sender, NalaiCoreInfo nalaiCoreInfo)
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

        [RelayCommand]
        private void OnSearch(object? parameter)
        {
            if (parameter is not string searchText) return;

            DownloadViewItems.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateDownloadCollection();
            }
            else
            {
                var items = NalaiDownService.GlobalDownloadTasks;
                var filteredItems = items.Where((pair, index) =>
                    pair.Value != null && pair.Value.FileName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

                // 将过滤后的数据添加到显示列表
                foreach (var item in filteredItems)
                {
                    if (item.Value != null) DownloadViewItems.Add(item.Value);
                }
            }
        }

        private void UpdateSearchedItems(string searchText)
        {
            DownloadViewItems.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateDownloadCollection();
            }
            else
            {
                var items = NalaiDownService.GlobalDownloadTasks;
                var filteredItems = items.Where((pair, index) =>
                    pair.Value != null && pair.Value.FileName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

                // 将过滤后的数据添加到显示列表
                foreach (var item in filteredItems)
                {
                    if (item.Value != null) DownloadViewItems.Add(item.Value);
                }
            }
        }

        [RelayCommand]
        private void OnSort(object? parameter)
        {
            if (parameter is not string sortType) return;
            DownloadViewItems = sortType switch
            {
                "FileNameAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.FileName)
                    .Select(pair => pair.Value)!),
                "FileNameDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.FileName)
                    .Select(pair => pair.Value)!),
                "FileSizeAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.TotalBytes)
                    .Select(pair => pair.Value)!),
                "FileSizeDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.TotalBytes)
                    .Select(pair => pair.Value)!),
                "StatusAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.Status)
                    .Select(pair => pair.Value)!),
                "StatusDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.Status)
                    .Select(pair => pair.Value)!),
                "SpeedAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.BytesPerSecondSpeed)
                    .Select(pair => pair.Value)!),
                "SpeedDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.BytesPerSecondSpeed)
                    .Select(pair => pair.Value)!),
                "ProgressAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.Progress)
                    .Select(pair => pair.Value)!),
                "ProgressDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.Progress)
                    .Select(pair => pair.Value)!),
                "CreatedTimeAsc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.SortableCreatedTime)
                    .Select(pair => pair.Value)!),
                "CreatedTimeDesc" => new ObservableCollection<CoreTask>(NalaiDownService.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.SortableCreatedTime)
                    .Select(pair => pair.Value)!),
                _ => DownloadViewItems
            };

            SortTypeText = sortType switch
            {
                "FileNameAsc" => "文件名",
                "FileNameDesc" => "文件名",
                "FileSizeAsc" => "文件大小",
                "FileSizeDesc" => "文件大小",
                "StatusAsc" => "状态",
                "StatusDesc" => "状态",
                "SpeedAsc" => "速度",
                "SpeedDesc" => "速度",
                "ProgressAsc" => "进度",
                "ProgressDesc" => "进度",
                _ => SortTypeText
            };

            SortTypeIcon = sortType switch
            {
                "FileNameAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "FileNameDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "FileSizeAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "FileSizeDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "StatusAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "StatusDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "SpeedAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "SpeedDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "ProgressAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "ProgressDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                _ => SortTypeIcon
            };
        }
    }
}