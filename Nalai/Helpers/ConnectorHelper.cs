using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.CoreConnector.Services;
using Nalai.Services;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public static class ConnectorHelper
{
    public static event EventHandler<NalaiCoreInfo> GlobalTaskUpdated;

    public static void Start()
    {
        WebSocketService.OnMessageReceived += OnWebSocketMessageReceived;
    }

    private static void OnWebSocketMessageReceived(object? sender, WsEvent<object> e)
    {
        switch (e.EventType)
        {
            case "DownloadProgress":
            {
                // TODO: 优化此处的双重序列化垃圾代码
                var str = JsonConvert.SerializeObject(e.Data);
                var data = JsonConvert.DeserializeObject<NalaiCoreInfo>(str);
                // if (data != null) CoreTask.ExternalUpdateInfoById(data.Id, data);
                if (data != null) GlobalTaskUpdated.Invoke(null, data);
                break;
            }
            case "DownloadComplete":
                break;
            case "Raw":
                Console.WriteLine(e.Data);
                break;
        }
    }

    public static async void StartTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        await CoreService.SendStartMsgAsync(task.Url, task.SaveDir, task.FileName, taskId, task.Headers);
    }

    public static async void StopTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        await CoreService.SendStopMsgAsync(taskId);
    }

    public static async void PauseOrResumeTaskById(string taskId)
    {
        await CoreService.SendSorcMsgAsync(taskId);
    }

    public static async void DeleteTaskById(string taskId)
    {
        var task = NalaiDownService.GlobalDownloadTasks.First(t => t.Value?.Id == taskId).Value;
        if (task == null)
            throw new ArgumentException("Task not found");

        await CoreService.SendDeleteMsgAsync(taskId);
    }
}