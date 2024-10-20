namespace Nalai.ViewModels.Windows;

public partial class DownloadingWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "Downloading...";

    [ObservableProperty] private string _fileName = "Unknown";
    public double Progress { get; set; }
}