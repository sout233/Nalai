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

    [JsonIgnore] public DownloadStatusKind Kind { get; set; }

    private string _statusKindRaw;

    [JsonProperty("kind")]
    public string StatusKindRaw
    {
        get => _statusKindRaw;
        set
        {
            _statusKindRaw = value;
            var kind = value switch
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
            Kind = kind;
        }
    }


    [JsonProperty("msg")] public string Message { get; set; }
}