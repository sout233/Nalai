﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
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
}