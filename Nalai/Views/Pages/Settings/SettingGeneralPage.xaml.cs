using System.Windows.Controls;
using Nalai.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Nalai.Views.Pages.Settings;

public partial class SettingGeneralPage : INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public SettingGeneralPage(SettingsViewModel vm)
    {
        ViewModel = vm;
        DataContext = this;

        InitializeComponent();
    }
}