using System.Text.Json.Serialization;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;

namespace Nalai.Models;

public class ExtendedChunkItem : ChunksItem
{
    public ExtendedChunkItem(ChunksItem item)
    {
        this.Index = item.Index;
        this.Size = item.Size;
        this.DownloadedBytes = item.DownloadedBytes;
    }
    
    [JsonIgnore] public string SizeText => ByteSizeFormatter.FormatSize(this.Size);

    [JsonIgnore] public string DownloadedSizeText => ByteSizeFormatter.FormatSize(this.DownloadedBytes);

    [JsonIgnore] public float Progress => ((float)this.DownloadedBytes / this.Size)*100;
    
    [JsonIgnore] public string ProgressText => $"{this.Progress:F}%";
}