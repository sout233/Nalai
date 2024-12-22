using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public enum DownloadStatusKind
{
    NoStart,
    Running,
    Pending,
    Error,
    Finished,
    Cancelled,
}

public class DownloadStatus
{
    public DownloadStatus(DownloadStatusKind kind, string message)
    {
        Kind = kind;
        Message = message;
    }

    [JsonIgnore]
    public DownloadStatusKind Kind
    {
        get
        {
            var kind = KindRaw switch
            {
                "NoStart" => DownloadStatusKind.NoStart,
                "Running" => DownloadStatusKind.Running,
                "Error" => DownloadStatusKind.Error,
                "Finished" => DownloadStatusKind.Finished,
                "DownloadFinished" => DownloadStatusKind.Finished,
                "Pending(Initializing)" => DownloadStatusKind.Pending,
                "Pending(Starting)" => DownloadStatusKind.Pending,
                "Pending(Stopping)" => DownloadStatusKind.Pending,
                "Cancelled" => DownloadStatusKind.Cancelled,
                _ => DownloadStatusKind.NoStart
            };

            return kind;
        }
        private set
        {
            KindRaw = value switch
            {
                DownloadStatusKind.NoStart => "NoStart",
                DownloadStatusKind.Running => "Running",
                DownloadStatusKind.Error => "Error",
                DownloadStatusKind.Finished => "Finished",
                DownloadStatusKind.Pending => "Pending(Initializing)",
                DownloadStatusKind.Cancelled => "Cancelled",
                _ => "NoStart"
            };
        }
    }

    [JsonProperty("kind")]
    public string? KindRaw { get; set; }


    [JsonProperty("message")] public string Message { get; set; }
}