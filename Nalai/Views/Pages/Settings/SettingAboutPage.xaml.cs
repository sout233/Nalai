using System.Windows.Controls;
using Nalai.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Nalai.Views.Pages.Settings;

public partial class SettingAboutPage : INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }
    
    public SettingAboutPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
}