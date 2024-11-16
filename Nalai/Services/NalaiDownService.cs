using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.Models;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static List<CoreTask?> GlobalDownloadTasks { get; set; } = new();

    public static event EventHandler<CoreTask>? GlobalDownloadTasksChanged;

    public static Task<CoreTask> NewTask(string url, string fileName, string path)
    {
        var task = new CoreTask(url, path);
        GlobalDownloadTasks.Add(task);

        Console.WriteLine($"Starting download: {fileName},\n from: {url},\n to: {path}");

        // SqlService.InsertOrUpdate(task);

        _ = task.StartDownloadAsync();

        GlobalDownloadTasksChanged?.Invoke(null, task);

        return Task.FromResult(task);
    }
}