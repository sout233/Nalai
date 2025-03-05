using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.CoreConnector.Services;
using Nalai.Models;
using Nalai.Services;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public static class ConnectorHelper
{
    public static event EventHandler<NalaiCoreInfo>? GlobalTaskProgressUpdated;
    public static event EventHandler<NalaiCoreInfo>? GlobalTaskStatusUpdated;

    public static void Start()
    {
        WebSocketService.OnMessageReceived += OnWebSocketMessageReceived;
    }

    private static void OnWebSocketMessageReceived(object? sender, WsEvent<object> e)
    {
        switch (e.EventType)
        {
            case "DownloadProgressChanged":
            {
                // TODO: 优化此处的双重序列化垃圾代码
                var str = JsonConvert.SerializeObject(e.Data);
                var data = JsonConvert.DeserializeObject<NalaiCoreInfo>(str);
                // if (data != null) CoreTask.ExternalUpdateInfoById(data.Id, data);
                if (data != null)
                {
                    GlobalTaskProgressUpdated?.Invoke(null, data);
                    var keyValuePairs = NalaiDownService.GlobalDownloadTasks.First(t => t.Value.Id == data.Id);
                    NalaiDownService.GlobalDownloadTasks[keyValuePairs.Key] = new NalaiCoreInfoExtended(data);
                }

                break;
            }
            case "DownloadStatusChanged":
            {
                var str = JsonConvert.SerializeObject(e.Data);
                var data = JsonConvert.DeserializeObject<NalaiCoreInfo>(str);
                // if (data != null) CoreTask.ExternalUpdateInfoById(data.Id, data);
                if (data != null)
                {
                    GlobalTaskStatusUpdated?.Invoke(null, data);
                    var keyValuePairs = NalaiDownService.GlobalDownloadTasks.First(t => t.Value.Id == data.Id);
                    NalaiDownService.GlobalDownloadTasks[keyValuePairs.Key] = new NalaiCoreInfoExtended(data);
                }
                break;
            }
            case "Raw":
                Console.WriteLine(e.Data);
                break;
        }
    }

    public static async Task StartTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        await CoreService.SendStartMsgAsync(task.Url, task.SaveDirectory, task.FileName, taskId, task.Headers);
        
    }

    public static async Task StopTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        var result = await CoreService.SendStopMsgAsync(taskId);
    }

    public static async Task<bool> PauseOrResumeTaskById(string taskId)
    {
        var result = await CoreService.SendSorcMsgAsync(taskId);

        var task = await CoreService.GetStatusAsync(taskId);

        if (task != null) GlobalTaskStatusUpdated?.Invoke(null, task);

        return result is { IsRunning: true };
    }


    public static async Task DeleteTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        await CoreService.SendDeleteMsgAsync(taskId);
        GlobalTaskStatusUpdated?.Invoke(null, task);
    }

    public static async Task SyncAllTasksFromCore()
    {
        var tasks = await CoreService.GetAllInfo();
        if (tasks != null)
        {
            var newDict = new Dictionary<string, NalaiCoreInfoExtended>();
            foreach (var (id, info) in tasks)
            {
                newDict.Add(id, new NalaiCoreInfoExtended(info));
            }

            NalaiDownService.GlobalDownloadTasks = newDict;
        }
    }

    public static async Task<NalaiCoreInfoExtended> CreateNewTask(string url, string saveDir, string fileName,
        Dictionary<string, string>? headers = null)
    {
        var id = CalculateNalaiCoreId.FromFileNameAndSaveDir(fileName, saveDir);

        _ = await CoreService.SendStartMsgAsync(url, saveDir, fileName, id, headers);

        // var info = await CoreService.GetStatusAsync(id);
        var info = new NalaiCoreInfo()
        {
            Id = id,
            Url = url,
            SaveDirectory = saveDir,
            FileName = fileName,
            Headers = headers ?? [],
        };

        if (info == null) throw new ArgumentException("Cannot get task info from core when creat new task");

        var task = new NalaiCoreInfoExtended(info);
        return task;
    }
}