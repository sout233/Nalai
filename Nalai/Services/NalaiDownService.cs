using System.Text;
using Nalai.Helpers;
using Nalai.Models;
using Newtonsoft.Json;

namespace Nalai.Services;

public static class NalaiDownService
{
    public static Dictionary<string, CoreTask?> GlobalDownloadTasks { get; internal set; } = [];
    public static Dictionary<string, CoreTask?> ListeningTasks { get; internal set; } = [];

    public static Task<CoreTask> NewTask(string url, string saveDir, string fileName,
        Dictionary<string, string>? headers = null)
    {
        var id = CalculateNalaiCoreId.FromFileNameAndSaveDir(saveDir, fileName);

        var task = new CoreTask(url, saveDir, fileName, id, headers);

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