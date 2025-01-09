using System.Windows.Controls;
using Nalai.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Nalai.Views.Pages.Settings;

public partial class SettingDownloadPage: INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public SettingDownloadPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
}