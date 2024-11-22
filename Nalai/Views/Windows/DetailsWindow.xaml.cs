using Nalai.Models;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DetailsWindow : FluentWindow
{
    public DetailsWindowViewModel ViewModel { get; }
    
    public DetailsWindow(CoreTask task)
    {
        InitializeComponent();
        ViewModel = new DetailsWindowViewModel(task);
        DataContext = this;
    }
}