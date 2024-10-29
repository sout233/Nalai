using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class DownloadCompleteWindowViewModel:ObservableObject
{
    public required DownloadCompleteWindow BindWindow { get; set; }
    
    [ObservableProperty] private string _applicationTitle="Download Complete";
    [ObservableProperty] private string _fileName = "Unknown";
    [ObservableProperty] private string _downloadPath = "Unknown";
    
    [RelayCommand]
    private void CloseWindow()
    {
        BindWindow.Close();
    }
}