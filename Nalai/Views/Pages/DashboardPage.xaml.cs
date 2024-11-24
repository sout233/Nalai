using System.Windows.Input;
using Nalai.Models;
using Nalai.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Nalai.Views.Pages;

public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        UpdatePauseOrResumeElement();
    }

    private void DownloadTaskListView_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        UpdatePauseOrResumeElement();
    }

    private void UpdatePauseOrResumeElement()
    {
        var item = DownloadTaskListView.SelectedItem;

        if (DownloadTaskListView.SelectedItems.Count > 1)
        {
            ViewModel.SetPauseOrResumeButtonEnabled(false);
            return;
        }

        if (item is not CoreTask task)
        {
            ViewModel.SetPauseOrResumeButtonEnabled(false);
            return;
        }

        ViewModel.UpdatePauseOrResumeElement(task.Status);
        ViewModel.SetPauseOrResumeButtonEnabled(true);
    }
}