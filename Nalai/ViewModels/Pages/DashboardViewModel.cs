using Nalai.Views.Windows;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _counter = 0;

        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
            var window = new DownloadingWindow();
            window.ShowDialog();
        }
    }
}
