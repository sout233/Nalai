using System.Collections.ObjectModel;
using Nalai.CoreConnector.Models;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "Nalai";

        // [ObservableProperty]
        // private ObservableCollection<object> _menuItems = new()
        // {
        //     new NavigationViewItem()
        //     {
        //         Content = "Home",
        //         Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
        //         TargetPageType = typeof(Views.Pages.DashboardPage)
        //     },
        //     new NavigationViewItem()
        //     {
        //         Content = "Data",
        //         Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
        //         TargetPageType = typeof(Views.Pages.DataPage)
        //     }
        // };
        //
        // [ObservableProperty]
        // private ObservableCollection<object> _footerMenuItems = new()
        // {
        //     new NavigationViewItem()
        //     {
        //         Content = "Settings",
        //         Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
        //         TargetPageType = typeof(Views.Pages.SettingsPage)
        //     }
        // };
        
        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        [ObservableProperty] private String _runningState;
        private HealthChecker Hc = new();
        public MainWindowViewModel()
        {
            Hc.Start();
            Console.WriteLine(RunningState);
            Hc.StatusChanged += UpdateCoreState;
        }
        private void UpdateCoreState(object sender,EventArgs e)
        {
            RunningState = Hc.Status switch
            {
                HealthStatus.Running => "Success",
                HealthStatus.Unknown => "Critical",
                _ => ""
            };
        }
    }
}
