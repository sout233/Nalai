using Nalai.Models;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadCompleteWindow : FluentWindow
{
    public DownloadCompleteWindowViewModel ViewModel { get; set; }
    public DownloadTask Task { get; set; }
    
    public DownloadCompleteWindow(DownloadCompleteWindowViewModel viewModel,DownloadTask task)
    {
        ViewModel = viewModel;
        Task = task;
        
        InitializeComponent();
    }
}