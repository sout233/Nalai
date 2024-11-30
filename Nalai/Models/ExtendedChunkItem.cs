using System.Text.Json.Serialization;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;

namespace Nalai.Models;

public class ExtendedChunkItem : ChunksItem
{
    [JsonIgnore] public string SizeText => ByteSizeFormatter.FormatSize(this.Size);

    [JsonIgnore] public string DownloadedSizeText => ByteSizeFormatter.FormatSize(this.DownloadedBytes);

    [JsonIgnore] public float Progress => ((float)this.DownloadedBytes / this.Size)*100;
    
    [JsonIgnore] public string ProgressText => $"{this.Progress:F}%";
}