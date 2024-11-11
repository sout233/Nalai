using System.ComponentModel;
using Nalai.CoreConnector.Models;

namespace Nalai.Models;

public class CoreTask
{
    public GetStatusResult? StatusResult { get; set; }
    public string FileName { get; set; } = "Unknown";
    public string SavePath { get; set; }
    public string Url { get; set; }
    public string? Id { get; set; }

    public List<Window> BindWindows { get; set; } = [];

    public event EventHandler<GetStatusResult> StatusChanged;
    public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;

    public CoreTask(string url, string savePath)
    {
        Url = url;
        SavePath = savePath;
    }

    public async Task StartDownload()
    {
        var result = await CoreConnector.CoreService.StartAsync(Url, SavePath);
        Id = result?.Id;
        StartListen();
    }

    public async Task StopAsync()
    {
        if (Id != null)
        {
            await CoreConnector.CoreService.StopAsync(Id);
        }
    }

    private void StartListen()
    {
        Task.Run(async () =>
        {
            while (StatusResult?.StatusText != "Finished")
            {
                var result = await CoreConnector.CoreService.GetStatusAsync(Id);

                if (result?.StatusText != StatusResult?.StatusText)
                {
                    StatusChanged?.Invoke(this, StatusResult);
                }

                if (result?.DownloadedBytes != StatusResult?.DownloadedBytes)
                {
                    if (result != null)
                    {
                        Console.WriteLine("Invoke");
                        ProgressChanged?.Invoke(this,
                            new DownloadProgressChangedEventArgs(totalBytesToReceive: result.TotalSize,
                                bytesReceived: result.DownloadedBytes,
                                progressPercentage: (float)result.DownloadedBytes / result.TotalSize * 100,
                                bytesPerSecondSpeed: result.BytesPerSecondSpeed)
                        );
                    }
                }

                StatusResult = result;

                Console.WriteLine(
                    $"Status: {StatusResult?.StatusText}, Downloaded: {StatusResult?.DownloadedBytes} / {StatusResult?.TotalSize}");

                await Task.Delay(1000);
            }
        });
    }
}