using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static List<DownloadTask> GlobalDownloadTasks { get; set; } = new List<DownloadTask>();

    public static async Task<DownloadTask> NewTask(string url, string fileName, string path)
    {
        DownloadTask task = new DownloadTask(url, fileName, path);
        GlobalDownloadTasks.Add(task);

        task.Downloader.DownloadStarted += OnDownloadStarted;
        task.Downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
        task.Downloader.DownloadProgressChanged += OnDownloadProgressChanged;
        task.Downloader.DownloadFileCompleted += OnDownloadFileCompleted;

        Debug.WriteLine("Starting download...");
        await task.StartDownload();
        
        return task;
    }

    private static void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        NalaiMsgBox.Show("Download completed");
    }

    private static void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {

    }

    private static void OnChunkDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
    }

    private static void OnDownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
    }
}