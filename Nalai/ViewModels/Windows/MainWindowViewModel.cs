using System.Collections.ObjectModel;
using System.Windows.Documents;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Models;
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

        [ObservableProperty] private string _runningState;
        [ObservableProperty] private string _runningTime;
        [ObservableProperty] private bool _isPaneOpen=false;
        public MainWindowViewModel()
        {
            RunningStateChecker.StatusChanged += UpdateCoreState;
            RunningStateChecker.TimeChanged += UpdateRunningTime;
            RunningStateChecker.Check();
            FormatState(RunningStateChecker.Status);
            UpdateRunningTime(null,new RunningStateChecker.TimeChangedEventArgs(0));//init
        }

        private void FormatState(HealthStatus s)
        {
            RunningState = RunningStateFormatter.Format(s);
        }
        private void UpdateCoreState(object sender,RunningStateChecker.StatusChangedEventArgs e)
        {
            FormatState(e.Status);
        }

        private void UpdateRunningTime(object sender,RunningStateChecker.TimeChangedEventArgs e)
        {
            RunningTime = TimeFormatter.ConvertSecondsToTimeCode(e.TimeStamp);
        }

        [RelayCommand]
        private void UpdateCore()
        {
            FormatState(RunningStateChecker.Status);
        }

        [RelayCommand]
        private void TestRunningStatus()
        {
            FormatState(RunningStateChecker.Status);
            Console.WriteLine($"Status Text: {RunningStateChecker.Status}");
            Console.WriteLine($"InfoBadge: {RunningState}");
        }
    }
}
