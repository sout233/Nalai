namespace Nalai.ViewModels.Windows;

public partial class DownloadingWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "Downloading...";

    [ObservableProperty] private string? _fileName = "Unknown";
    [ObservableProperty] private double _progressValue = 0;
    
    public void SetProgress(double value)
    {
        ProgressValue = value;
    }
}