using System.Collections.ObjectModel;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Resources;
using Nalai.Services;
using Nalai.Views.Windows;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private string _pauseOrResumeText = I18NHelper.GetTranslation("bt.pause");
        [ObservableProperty] private SymbolIcon _pauseOrResumeIcon = new() { Symbol = SymbolRegular.Pause24 };
        [ObservableProperty] private bool _isPauseOrResumeEnabled;
        [ObservableProperty] private SymbolIcon _pauseOrResumeButtonIcon = new() { Symbol = SymbolRegular.Pause24 };

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Visibility _searchPanelVisibility = Visibility.Collapsed;
        [ObservableProperty] private ControlAppearance _filterButtonAppearance = ControlAppearance.Secondary;

        partial void OnSearchTextChanging(string? value)
        {
            if (value != null) UpdateSearchedItems(value);
        }

        [ObservableProperty] private string _sortTypeText = I18NHelper.GetTranslation("sort.by.name");
        [ObservableProperty] private SymbolIcon _sortTypeIcon = new() { Symbol = SymbolRegular.ArrowDown24 };

        [ObservableProperty] private ObservableCollection<NalaiCoreInfoExtended> _downloadViewItems;

        public DashboardViewModel()
        {
            // ConnectorHelper.GlobalDownloadTasks = SqlService.ReadAll();
            UpdateDownloadCollection();

            // foreach (var (_, task) in ConnectorHelper.GlobalDownloadTasks)
            // {
            //     if (task != null) task.StatusChanged += OnDownloadStatusChanged;
            // }

            ConnectorHelper.GlobalTaskProgressUpdated += OnGlobalTaskProgressChanged;
            ConnectorHelper.GlobalTaskListUpdated += OnGlobalTaskListUpdated;
        }

        private void OnGlobalTaskListUpdated(object? sender, EventArgs e)
        {
            UpdateDownloadCollection();
        }

        public void SetPauseOrResumeButtonEnabled(bool isEnabled)
        {
            IsPauseOrResumeEnabled = isEnabled;
        }

        private void OnGlobalTaskProgressChanged(object? sender, NalaiCoreInfo nalaiCoreInfo)
        {
            UpdateDownloadCollection();
        }

        [RelayCommand]
        private Task OnNewTask()
        {
            var window = new NewTaskWindow();

            window.Show();

            UpdateDownloadCollection();

            return Task.CompletedTask;
        }

        private ObservableCollection<NalaiCoreInfoExtended> GenerateDownloadCollection()
        {
            var tasks = ConnectorHelper.GlobalDownloadTasks;
            var taskCollection = new ObservableCollection<NalaiCoreInfoExtended>();
            foreach (var (_, task) in tasks)
            {
                taskCollection.Add(task);
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

        public void UpdatePauseOrResumeElement(DownloadStatusKind status)
        {
            PauseOrResumeText = status switch
            {
                DownloadStatusKind.Running => I18NHelper.GetTranslation(LangKeys.Button_Pause),
                DownloadStatusKind.Pending => I18NHelper.GetTranslation(LangKeys.Button_Pause),
                DownloadStatusKind.NoStart => I18NHelper.GetTranslation(LangKeys.Button_Resume),
                DownloadStatusKind.Cancelled => I18NHelper.GetTranslation(LangKeys.Button_Resume),
                DownloadStatusKind.Finished => I18NHelper.GetTranslation(LangKeys.Button_ReDownload),
                DownloadStatusKind.Error => I18NHelper.GetTranslation(LangKeys.Button_ReDownload),
                _ => PauseOrResumeText
            };

            PauseOrResumeIcon = status switch
            {
                DownloadStatusKind.Running => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatusKind.Pending => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatusKind.NoStart => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatusKind.Cancelled => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatusKind.Finished => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatusKind.Error => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                _ => PauseOrResumeIcon
            };

            PauseOrResumeButtonIcon = status switch
            {
                DownloadStatusKind.Running => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatusKind.Pending => new SymbolIcon { Symbol = SymbolRegular.Pause24 },
                DownloadStatusKind.NoStart => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatusKind.Cancelled => new SymbolIcon { Symbol = SymbolRegular.Play24 },
                DownloadStatusKind.Finished => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                DownloadStatusKind.Error => new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                _ => PauseOrResumeButtonIcon
            };
        }

        [RelayCommand]
        private async Task OnPauseOrResume(object? parameter)
        {
            if (parameter is not ListView item) return;

            foreach (var coreTask in item.SelectedItems.OfType<NalaiCoreInfoExtended>())
            {
                // var result = await coreTask.StartOrCancelAsync();
                var result = await ConnectorHelper.PauseOrResumeTaskById(coreTask.Id);
                UpdatePauseOrResumeElement(result ? DownloadStatusKind.Running : coreTask.Status.Kind);
            }

            UpdateDownloadCollection();
        }

        [RelayCommand]
        private async Task OnRemove(object? parameter)
        {
            if (parameter is not ListView item) return;

            var tasksToDelete = item.SelectedItems.OfType<NalaiCoreInfoExtended>().ToList();

            foreach (var coreTask in tasksToDelete)
            {
                await ConnectorHelper.DeleteTaskById(coreTask.Id);
                // await coreTask.DeleteAsync();
                UpdatePauseOrResumeElement(coreTask.Status.Kind);
            }

            await ConnectorHelper.SyncAllTasksFromCore();
            UpdateDownloadCollection();
        }

        [RelayCommand]
        private async Task OnCancel(object? parameter)
        {
            if (parameter is not ListView item) return;

            foreach (var coreTask in item.SelectedItems.OfType<NalaiCoreInfoExtended>())
            {
                await ConnectorHelper.StopTaskById(coreTask.Id);
                // await coreTask.CancelAsync();
                UpdatePauseOrResumeElement(coreTask.Status.Kind);
            }

            UpdateDownloadCollection();
        }

        [RelayCommand]
        private void OnShowDetails(object? parameter)
        {
            if (parameter is not ListView item) return;
            if (item.SelectedItem is not NalaiCoreInfoExtended task) return;

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
                var items = ConnectorHelper.GlobalDownloadTasks;
                var filteredItems = items.Where((pair, _) =>
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
                var items = ConnectorHelper.GlobalDownloadTasks;
                var filteredItems = items.Where((pair, _) =>
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
                "FileNameAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.FileName)
                    .Select(pair => pair.Value)!),
                "FileNameDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.FileName)
                    .Select(pair => pair.Value)!),
                "FileSizeAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.TotalBytes)
                    .Select(pair => pair.Value)!),
                "FileSizeDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.TotalBytes)
                    .Select(pair => pair.Value)!),
                "StatusAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.Status)
                    .Select(pair => pair.Value)!),
                "StatusDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.Status)
                    .Select(pair => pair.Value)!),
                "SpeedAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.BytesPerSecondSpeed)
                    .Select(pair => pair.Value)!),
                "SpeedDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.BytesPerSecondSpeed)
                    .Select(pair => pair.Value)!),
                "ProgressAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.Progress)
                    .Select(pair => pair.Value)!),
                "ProgressDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.Progress)
                    .Select(pair => pair.Value)!),
                "CreatedTimeAsc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderBy(pair => pair.Value?.SortableCreatedTime)
                    .Select(pair => pair.Value)!),
                "CreatedTimeDesc" => new ObservableCollection<NalaiCoreInfoExtended>(ConnectorHelper.GlobalDownloadTasks
                    .OrderByDescending(pair => pair.Value?.SortableCreatedTime)
                    .Select(pair => pair.Value)!),
                _ => DownloadViewItems
            };

            SortTypeText = sortType switch
            {
                "FileNameAsc" => I18NHelper.GetTranslation("sort.by.name"),
                "FileNameDesc" => I18NHelper.GetTranslation("sort.by.name"),
                "FileSizeAsc" => I18NHelper.GetTranslation("sort.by.size"),
                "FileSizeDesc" => I18NHelper.GetTranslation("sort.by.size"),
                "CreatedTimeAsc" => I18NHelper.GetTranslation("sort.by.createdTime"),
                "CreatedTimeDesc" => I18NHelper.GetTranslation("sort.by.createdTime"),
                "StatusAsc" => I18NHelper.GetTranslation("sort.by.status"),
                "StatusDesc" => I18NHelper.GetTranslation("sort.by.status"),
                _ => SortTypeText
            };

            SortTypeIcon = sortType switch
            {
                "FileNameAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "FileNameDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "FileSizeAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "FileSizeDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "CreatedTimeAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "CreatedTimeDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                "StatusAsc" => new SymbolIcon { Symbol = SymbolRegular.ArrowDown24 },
                "StatusDesc" => new SymbolIcon { Symbol = SymbolRegular.ArrowUp24 },
                _ => SortTypeIcon
            };
        }

        [RelayCommand]
        private void OnToggleSearchPanelVisible()
        {
            SearchPanelVisibility = SearchPanelVisibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
            FilterButtonAppearance = SearchPanelVisibility == Visibility.Collapsed
                ? ControlAppearance.Secondary
                : ControlAppearance.Primary;
        }
    }
}