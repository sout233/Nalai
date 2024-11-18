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

    private void DownloadTaskListView_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        
    }

    private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var item = DownloadTaskListView.SelectedItem;
        if (item is CoreTask task)
        {
            ViewModel.UpdateRightClickMenu(task.InfoResult.Status);
        }
    }
}