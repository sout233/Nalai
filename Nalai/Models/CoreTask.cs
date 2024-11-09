﻿using System.ComponentModel;
using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;

namespace Nalai.Models;

public class CoreTask
{
    public GetStatusResult? StatusResult { get; set; }
    public string FileName { get; set; } = "Unknown";
    public string SavePath { get; set; }
    public string Url { get; set; }
    public string? Id { get; set; }

    public event EventHandler<GetStatusResult> StatusChanged;
    public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    public CoreTask(string url, string savePath)
    {
        Url = url;
        SavePath = savePath;
    }

    public async Task StartDownload()
    {
        var result = await PreCore.StartAsync(Url, SavePath);
        Id = result?.Id;
        StartListen();
    }

    private void StartListen()
    {
        Task.Run(async () =>
        {
            while (StatusResult?.Status != "Finished")
            {
                var result = await PreCore.GetStatusAsync(Id);

                StatusResult = result;

                if (result?.Status != StatusResult?.Status)
                {
                    StatusChanged?.Invoke(this, StatusResult);
                }

                if (result?.DownloadedBytes != StatusResult?.DownloadedBytes)
                {
                    if (result != null)
                        ProgressChanged?.Invoke(this,
                            new ProgressChangedEventArgs((int)(result.DownloadedBytes / result.TotalSize * 100), this));
                }

                await Task.Delay(1000);
            }
        });
    }
}