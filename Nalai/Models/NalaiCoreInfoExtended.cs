using Nalai.CoreConnector.Models;
using Nalai.Helpers;

namespace Nalai.Models;

public record NalaiCoreInfoExtended : NalaiCoreInfo
{
    public NalaiCoreInfoExtended(string id, NalaiCoreInfo coreInfo)
    {
        DownloadedBytes = coreInfo.DownloadedBytes;
        TotalBytes = coreInfo.TotalBytes;
        BytesPerSecondSpeed = coreInfo.BytesPerSecondSpeed;
        FileName = coreInfo.FileName;
        Url = coreInfo.Url;
        Status = coreInfo.Status;
        SaveDirectory = coreInfo.SaveDirectory;
        CreatedTime = coreInfo.CreatedTime;
        if (coreInfo.Chunks is not null)
            Chunks = coreInfo.Chunks.Select(c => new ExtendedChunkItem(c)).ToList();
        // Id = coreInfo.Id;
        Id = id;
        Headers = coreInfo.Headers;
    }

    public Dictionary<string, string> Headers { get; set; }
    public List<ExtendedChunkItem> Chunks { get; set; } = [];
    public string TotalSizeText => ByteSizeFormatter.FormatSize(TotalBytes);
    public string DownloadedSizeText => ByteSizeFormatter.FormatSize(DownloadedBytes);
    public string SpeedText => ByteSizeFormatter.FormatSize(BytesPerSecondSpeed) + "/s";
    public float Progress => (float)DownloadedBytes / TotalBytes * 100;
    public string ProgressText => Progress.ToString("0.00") + "%";
    private RustSystemTime RawCreatedTime => CreatedTime;
    public string CreatedTimeText => TimeFormatter.FormatRustSystemTime(RawCreatedTime);
    public DateTimeOffset SortableCreatedTime => TimeFormatter.ConvertRustSystemTime(RawCreatedTime);
    public TimeSpan Eta => TimeFormatter.CalculateRemainingTime(DownloadedBytes, TotalBytes, BytesPerSecondSpeed);
    public string EtaText => TimeFormatter.FormatTimeSpanReadable(Eta);
    public List<Window> BindWindows { get; set; } = [];
    public string StatusText => Status.Kind.ToString() ?? "Unknown";
    public string Id { get; set; }
}