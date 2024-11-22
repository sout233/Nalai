using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nalai.CoreConnector.Models;

namespace Nalai.CoreConnector;

public static class CoreService
{
    private static readonly HttpClient HttpClient = new();
    public static Process GlobalNalaiCoreProcess { get; set; }

    public static async Task StartAsync()
    {
        // 检查是否已经存在 nalai_core.exe 进程
        bool isProcessRunning = Process.GetProcessesByName("nalai_core").Length > 0;

        if (!isProcessRunning)
        {
            // 如果进程不存在，则启动它
            string pathToExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "nalai_core.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pathToExe,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // 异步读取标准输出
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    GlobalNalaiCoreProcess = process;

                    // 等待进程退出
                    await process.WaitForExitAsync();

                    // 输出结果
                    Console.WriteLine("Standard Output:");
                    Console.WriteLine(output);

                    Console.WriteLine("Standard Error:");
                    Console.WriteLine(error);
                }

                Console.WriteLine("nalai_core.exe 已启动并完成运行");
            }
            catch (Exception ex)
            {
                // 处理可能发生的异常，比如路径错误或文件不存在
                Console.WriteLine($"无法启动 nalai_core.exe: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("nalai_core.exe 正在运行中");
        }
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

    public static Dictionary<string, NalaiCoreInfo>? GetAllInfo()
    {
        return MakeHttpRequestWithRetry(
            () => HttpClient.GetAsync("http://localhost:13088/all_info"),
            async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GetAllInfoResponse>(content);
                return result?.Data;
            }).Result;
    }

    public static async Task<NalaiCoreDownloadResult?> SendStartMsgAsync(string url, string path)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/download")
        {
            Query = $"url={url}&save_dir={path}"
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

    private static async Task<T?> MakeHttpRequestWithRetry<T>(Func<Task<HttpResponseMessage>> requestAction, Func<HttpResponseMessage, Task<T>> parseResponse) where T : class?
    {
        const int maxRetries = 2;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                HttpResponseMessage response = await requestAction();
                response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299

                // 解析响应内容
                return await parseResponse(response);
            }
            catch (HttpRequestException hre)
            {
                if (attempt == maxRetries)
                {
                    // 所有重试均失败，调用StartAsync启动内核
                    await StartAsync();
                    throw; // 重新抛出异常
                }
                else
                {
                    Console.WriteLine($"请求失败，正在尝试第{attempt + 1}次重试: {hre.Message}");
                    await Task.Delay(1000); // 重试前等待一段时间
                }
            }
        }

        return null;
    }
}