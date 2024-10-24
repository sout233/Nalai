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
    [ObservableProperty] private IEnumerable<ChunkValue> _chunkValues = new List<ChunkValue>();
    [ObservableProperty] private ObservableCollection<ProgressData> _progressBars = new();

    public DownloadingWindowViewModel()
    {
        AddProgressBars();
    }

    public class ProgressData
    {
        public int Value { get; set; }
        public int Maximum { get; set; }
        public int Index { get; set; }
    }

    public void AddProgressBars()
    {
        for (int i = 0; i < 5; i++)
        {
            ProgressBars.Add(new ProgressData { Value = 0, Maximum = 100,Index = i});
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

    public void SetChunkValues(IEnumerable<ChunkValue> values)
    {
        ChunkValues = values;
    }
}