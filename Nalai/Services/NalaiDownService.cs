using Nalai.Helpers;
using Nalai.Models;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static Dictionary<string, CoreTask?> GlobalDownloadTasks { get; internal set; } = [];

    public static Task<CoreTask> NewTask(string url, string saveDir, string fileName)
    {
        var task = new CoreTask(url, saveDir, fileName);
        var id = CalculateNalaiCoreId.FromFileNameAndSaveDir(saveDir, fileName);

        if (GlobalDownloadTasks.ContainsKey(id))
        {
            GlobalDownloadTasks[id] = task;
        }
        else
        {
            GlobalDownloadTasks.TryAdd(id, task);
        }

        Console.WriteLine($"Starting download: {fileName},\n from: {url},\n to: {saveDir}");

        // SqlService.InsertOrUpdate(task);

        _ = task.StartDownloadAsync();

        return Task.FromResult(task);
    }
}