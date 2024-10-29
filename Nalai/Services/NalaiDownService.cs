﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Views.Windows;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

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

        return Task.FromResult(task);
    }

    private static void CloseTaskBindWindows(DownloadTask task)
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
        task.Downloader.CancelAsync();
        GlobalDownloadTasks.Remove(task);
    }

    public static void CancelTask(DownloadTask task)
    {
        task.Package = task.Downloader.Package;
        task.Status = DownloadStatus.Stopped;
        CloseTaskBindWindows(task);
        task.Downloader.CancelAsync();
    }
}