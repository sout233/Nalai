﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Downloader;
using Nalai.Models;
using Nalai.Views.Windows;

// TODO: 宏不是好文明，建议重构改Service的异步方法
#pragma warning disable CS4014 // 别再用await了

namespace Nalai.Services;

public static class NalaiDownService
{
    public static List<DownloadTask?> GlobalDownloadTasks { get; set; } = new();

    public static DownloadTask? GetTaskByUrl(string url)
    {
        return GlobalDownloadTasks.FirstOrDefault(x => x?.Url == url);
    }

    public static Task<DownloadTask> NewTask(string url, string fileName, string path)
    {
        var task = new DownloadTask(url, fileName, path);
        GlobalDownloadTasks.Add(task);

        Console.WriteLine($"Starting download: {fileName},\n from: {url},\n to: {path}");
        task.StartDownload();

        SqlService.InsertOrUpdate(task);

        return Task.FromResult(task);
    }

    public static void CloseTaskBindWindows(DownloadTask task)
    {
        foreach (var window in task.BindWindows)
        {
            if (window is DownloadingWindow downloadingWindow)
            {
                Console.WriteLine($"Closing window: {downloadingWindow.ViewModel.ApplicationTitle}");
                downloadingWindow.Close();
            }
        }
    }

    public static void RemoveTask(DownloadTask task)
    {
        CloseTaskBindWindows(task);
        task.Downloader?.CancelAsync();
        GlobalDownloadTasks.Remove(task);
                
        SqlService.Delete(task);
    }

    public static void StopTask(DownloadTask task)
    {
        task.Package = task.Downloader?.Package;
        task.Status = DownloadStatus.Stopped;
        CloseTaskBindWindows(task);
        task.Downloader?.CancelAsync();
        
        
        SqlService.InsertOrUpdate(task);
    }
}