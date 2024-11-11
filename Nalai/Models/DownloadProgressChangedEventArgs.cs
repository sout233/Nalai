namespace Nalai.Models;

using System;

public class DownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, float progressPercentage, long bytesPerSecondSpeed)
    : EventArgs
{
    public long BytesReceived { get; set; } = bytesReceived;
    public long TotalBytesToReceive { get; set; } = totalBytesToReceive;
    public float ProgressPercentage { get; set; } = progressPercentage;
    public long BytesPerSecondSpeed { get; set; } = bytesPerSecondSpeed;
}