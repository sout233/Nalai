using Nalai.Helpers;
using Nalai.Models;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static Dictionary<string, CoreTask?> GlobalDownloadTasks { get; internal set; } = [];
    public static List<CoreTask> ListeningTasks { get; internal set; } = new();

    public static Task<CoreTask> NewTask(string url, string fileName, string path)
    {
        var task = new CoreTask(url, path);
        GlobalDownloadTasks.Add(CalculateNalaiCoreId.FromFileNameAndSaveDir(url, path), task);

        Console.WriteLine($"Starting download: {fileName},\n from: {url},\n to: {path}");

        // SqlService.InsertOrUpdate(task);

        _ = task.StartDownloadAsync();

        return Task.FromResult(task);
    }

    public static void InsertListeningTask(CoreTask task)
    {
        var id = task.Id;
        if (ListeningTasks.Any(t => t?.Id == id))
        {
            return;
        }

        ListeningTasks.Add(task);
    }

    public static void RemoveListeningTask(CoreTask task)
    {
        var id = task.Id;
        var index = ListeningTasks.FindIndex(t => t?.Id == id);
        if (index >= 0)
        {
            ListeningTasks.RemoveAt(index);
        }
    }
}