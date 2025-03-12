using Nalai.CoreConnector.Models;

namespace Nalai.ViewModels.Windows;

public partial class DownloadErrorWindowViewModel: ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "Error";
    [ObservableProperty] private string _fileName = "Unknown";
    [ObservableProperty] private string _errorMessage = "Unknown error";
    [ObservableProperty] private string _url = "Unknown URL";
    
    public NalaiCoreInfo CoreInfo { get; set; }

    public DownloadErrorWindowViewModel(NalaiCoreInfo coreInfo)
    {
        CoreInfo = coreInfo;
        
        ApplicationTitle = "Error: "+coreInfo.FileName;
        FileName = coreInfo.FileName;
        Url = coreInfo.Url;
        ErrorMessage = coreInfo.Status.Message;
    }
}