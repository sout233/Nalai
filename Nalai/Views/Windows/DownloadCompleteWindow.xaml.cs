using Nalai.Models;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadCompleteWindow : FluentWindow
{
    public DownloadCompleteWindowViewModel ViewModel { get; }
    public NalaiCoreInfoExtended Task { get; set; }
    
    public DownloadCompleteWindow(DownloadCompleteWindowViewModel viewModel,NalaiCoreInfoExtended task)
    {
        ViewModel = viewModel;
        DataContext = this;
        Task = task;
        
        InitializeComponent();
    }
}