using Nalai.Models;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static List<CoreTask?> GlobalDownloadTasks { get; internal set; } = [];


    public static Task<CoreTask> NewTask(string url, string fileName, string path)
    {
        var task = new CoreTask(url, path);
        GlobalDownloadTasks.Add(task);

        Console.WriteLine($"Starting download: {fileName},\n from: {url},\n to: {path}");

        // SqlService.InsertOrUpdate(task);

        _ = task.StartDownloadAsync();

        return Task.FromResult(task);
    }
}