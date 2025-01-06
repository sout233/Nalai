using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Nalai.CoreConnector.Models;
using Nalai.Launcher;

namespace Nalai.CoreConnector;

public static class CoreService
{
    private static readonly HttpClient HttpClient = new();

    public static Task StartAsync()
    {
        // 检查是否已经存在 nalai_core.exe 进程
        var isProcessRunning = Process.GetProcessesByName("nalai_core").Length > 0;

        if (!isProcessRunning)
        {
            // 如果进程不存在，则启动它
            var pathToExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "nalai_core.exe");

            try
            {
                JobLauncher.LaunchExe(pathToExe, [], false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法启动 nalai_core.exe: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("nalai_core.exe 正在运行中");
        }

        return Task.CompletedTask;
    }

    public static void SendExitMsg()
    {
        try
        {
            var uriBuilder = new UriBuilder("http://localhost:13088/exit");
            _ = HttpClient.GetAsync(uriBuilder.Uri).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"无法发送退出消息: {ex.Message}");
        }
    }

    public static async Task<Dictionary<string, NalaiCoreInfo>?> GetAllInfo()
    {
        return await MakeHttpRequestWithRetry(
            () => HttpClient.GetAsync("http://localhost:13088/all_info"),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GetAllInfoResponse>(content);
                return result?.Data;
            });
    }

    public static async Task<NalaiCoreDownloadResult?> SendStartMsgAsync(string url, string saveDir, string fileName,
        string id, Dictionary<string, string>? headers)
    {
        var headersJson = headers == null ? "{}" : JsonConvert.SerializeObject(headers);
        var headersBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(headersJson));

        var uriBuilder = new UriBuilder("http://localhost:13088/download")
        {
            Query = $"url={url}&save_dir={saveDir}&file_name={fileName}&id={id}&headers={headersBase64}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");

        return await MakeHttpRequestWithRetry(
            () => HttpClient.PostAsync(uriBuilder.Uri, null),
            async response =>
            {
                var contentResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PostDownloadResponse>(contentResult);
                return result?.Data;
            });
    }

    public static async Task<NalaiCoreInfo?> GetStatusAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/info")
        {
            Query = $"id={id}"
        };

        return await MakeHttpRequestWithRetry(
            () => HttpClient.GetAsync(uriBuilder.Uri),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GetInfoResponse>(content);
                return result?.Data;
            });
    }

    public static async Task<NalaiSorcResult?> SendSorcMsgAsync(string id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/sorc")
        {
            Query = $"id={id}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");

        return await MakeHttpRequestWithRetry(
            () => HttpClient.PostAsync(uriBuilder.Uri, null),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PostSorcResponse>(content);
                return result?.Data;
            });
    }

    public static async Task<NalaiStopResult?> SendStopMsgAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/cancel")
        {
            Query = $"id={id}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");

        return await MakeHttpRequestWithRetry(
            () => HttpClient.PostAsync(uriBuilder.Uri, null),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PostStopResponse>(content);
                return result?.Data;
            });
    }

    public static async Task<Dictionary<string, NalaiCoreInfo>?> SendDeleteMsgAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/download")
        {
            Query = $"id={id}"
        };

        return await MakeHttpRequestWithRetry(
            () => HttpClient.DeleteAsync(uriBuilder.Uri),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GetAllInfoResponse>(content);
                return result?.Data;
            });
    }

    private static async Task<T?> MakeHttpRequestWithRetry<T>(Func<Task<HttpResponseMessage>> requestAction,
        Func<HttpResponseMessage, Task<T>> parseResponse) where T : class?
    {
        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var response = await requestAction();
                response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299

                // 解析响应内容
                return await parseResponse(response);
            }
            catch (HttpRequestException hre)
            {
                if (attempt == maxRetries)
                {
                    throw; // 所有重试均失败， 重新抛出异常
                }

                Console.WriteLine($"请求失败，正在尝试第{attempt + 1}次重试: {hre.Message}");
                await Task.Delay(1000); // 重试前等待一段时间

                // 调用StartAsync启动内核
                await StartAsync();
            }
        }

        return null;
    }
}