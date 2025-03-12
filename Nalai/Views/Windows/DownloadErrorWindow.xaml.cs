using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadErrorWindow : FluentWindow
{
    public DownloadErrorWindowViewModel ViewModel { get; }
    
    public DownloadErrorWindow(DownloadErrorWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
}