using System.Collections.ObjectModel;
using Nalai.Models;

namespace Nalai.ViewModels.Windows;

public partial class
    DownloadingWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "Downloading...";

    [ObservableProperty] private string? _fileName = "Unknown";
    [ObservableProperty] private double _progressValue = 0;
    [ObservableProperty] private string _progressText = "0%";
    [ObservableProperty] private string _downloadSpeed = "0 B/s";
    [ObservableProperty] private string _fileSize = "Unknown";
    [ObservableProperty] private string _remainingTime = "Unknown";
    [ObservableProperty] private ObservableCollection<ChunkProgressData> _chunkProgressBars = new();

    public DownloadingWindowViewModel()
    {
    }


    public void AddOrUpdateChunkProgressBars(string id, float value)
    {
        var chunk = ChunkProgressBars.FirstOrDefault(x => x.Id == id);
        if (chunk == null)
        {
            chunk = new ChunkProgressData { Id = id, Value = value };
            ChunkProgressBars.Add(chunk);
        }
        else
        {
            ChunkProgressBars[ChunkProgressBars.IndexOf(chunk)].Value = value;
            Console.WriteLine("Updated chunk progress bar: " + id + " - " + value);
        }
    }


    public void SetProgress(double value)
    {
        ProgressValue = value;
    }

    public void SetFileName(string? value)
    {
        FileName = value;
    }

    public void SetRemainingTime(string value)
    {
        RemainingTime = value;
    }

    public void SetFileSize(string value)
    {
        FileSize = value;
    }

    public void SetDownloadSpeed(string value)
    {
        DownloadSpeed = value;
    }

    public void SetProgressText(string value)
    {
        ProgressText = value;
    }
}