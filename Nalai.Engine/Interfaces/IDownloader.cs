using System.ComponentModel;
using Nalai.Engine.Models;

namespace Nalai.Engine.Interfaces;

public interface IDownloader : IDisposable
{
    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
    event EventHandler DownloadCompleted;
    event EventHandler<DownloadEvents.DownloadSpeedChangedEventArgs> DownloadSpeedChanged;

    Task DownloadFileAsync(string url, string outputPath);
    void Pause();
    void Resume();
    void Stop();
}