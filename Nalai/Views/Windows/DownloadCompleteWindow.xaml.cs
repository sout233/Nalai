using Nalai.Models;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadCompleteWindow : FluentWindow
{
    public DownloadCompleteWindowViewModel ViewModel { get; set; }
    public CoreTask Task { get; set; }
    
    public DownloadCompleteWindow(DownloadCompleteWindowViewModel viewModel,CoreTask task)
    {
        ViewModel = viewModel;
        DataContext = this;
        Task = task;
        
        InitializeComponent();
    }
}