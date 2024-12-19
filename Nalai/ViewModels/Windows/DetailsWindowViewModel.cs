using System.Collections.ObjectModel;
using Nalai.Models;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Windows;

public partial class DetailsWindowViewModel : ObservableObject
{
    public DetailsWindow BindWindow { get; set; }
    [ObservableProperty] private ObservableCollection<ExtendedChunkItem> _chunksCollection = [];
    [ObservableProperty] private string _applicationTitle = "Details: Unknown";
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _savePath;
    [ObservableProperty] private string _fileSize;
    [ObservableProperty] private string _status;
    [ObservableProperty] private string _downloadedSize;
    [ObservableProperty] private string _totalSize;
    [ObservableProperty] private string _speed;
    [ObservableProperty] private string _url;
    [ObservableProperty] private string _eta;
    [ObservableProperty] private string _createdTime;

    public DetailsWindowViewModel(CoreTask task,DetailsWindow window)
    {
        ApplicationTitle = "Details: " + task.FileName;
        BindWindow = window;
        
        ProgressText = task.RealtimeStatusText;
        FileName = task.FileName;
        Id = task.Id ?? string.Empty;
        SavePath = task.SaveDir;
        FileSize = $"{task.DownloadedSizeText} / {task.TotalBytes}";
        Status = task.Status.ToString();
        DownloadedSize = task.DownloadedSizeText;
        TotalSize = task.TotalSizeText;
        Speed = task.SpeedText;
        Url = task.Url;
        Eta = task.EtaText;
        CreatedTime = task.CreatedTimeText;
        ChunksCollection = new ObservableCollection<ExtendedChunkItem>(task.Chunks);
    }
    
    [RelayCommand]
    private void CloseWindow()
    {
        // TODO: Implement closing window for MVVM
        // Messenger.Default.Send(new CloseWindowMessage());
        BindWindow.Close();
    }
}