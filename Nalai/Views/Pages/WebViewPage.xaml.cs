using Nalai.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Nalai.Views.Pages;

public partial class WebViewPage : INavigableView<WebViewViewModel>
{
    public WebViewViewModel ViewModel { get; }
    public WebViewPage(WebViewViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        
    }
}