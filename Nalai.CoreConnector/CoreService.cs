using System.Diagnostics;
using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.CoreConnector;

public static class CoreService
{
    private static readonly HttpClient HttpClient = new();

    public static void Start()
    {
        // 检查是否已经存在 nalai_core.exe 进程
        bool isProcessRunning = Process.GetProcessesByName("nalai_core").Length > 0;

        if (!isProcessRunning)
        {
            // 如果进程不存在，则启动它
            string pathToExe = @"Tool\nalai_core.exe"; // 假设 nalai_core.exe 存在于 Core 文件夹下
            try
            {
                Process.Start(pathToExe);
                Console.WriteLine("nalai_core.exe 已启动");
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
    
    public static Dictionary<string, NalaiCoreInfo>? GetAllInfoDictionary()
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/all_info");
        var response = HttpClient.GetAsync(uriBuilder.Uri).Result;
        response.EnsureSuccessStatusCode();
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<GetAllInfoResponse>(content);
        
        if (result is { Success: false })
        {
            return null;
        }
        
        return result?.Data;
    }

    public static async Task<NalaiCoreDownloadResult?> SendStartMsgAsync(string url, string path)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/download")
        {
            Query = $"url={url}&save_dir={path}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
        var response = await HttpClient.PostAsync(uriBuilder.Uri, null);
        response.EnsureSuccessStatusCode();
        var contentResult = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PostDownloadResponse>(contentResult);
        
        if (result is { Success: false })
        {
            return null;
        }
        
        return result?.Data;
    }

    public static async Task<NalaiCoreInfo?> GetStatusAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/info")
        {
            Query = $"id={id}"
        };
        //HttpContent content = new();
        var response = await HttpClient.GetAsync(uriBuilder.Uri);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<GetInfoResponse>(content);

        if (result is { Success: false })
        {
            return null;
        }
            
        return result?.Data;
    }
    
    public static async Task<NalaiSorcResult?> SendSorcMsgAsync(string id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/sorc")
        {
            Query = $"id={id}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
        var response = await HttpClient.PostAsync(uriBuilder.Uri, null);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PostSorcResponse>(content);
        
        if (result is { Success: false })
        {
            return null;
        }
        
        return result?.Data;
    }

    public static async Task<NalaiStopResult?> SendStopMsgAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/cancel")
        {
            Query = $"id={id}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
        var response = await HttpClient.PostAsync(uriBuilder.Uri, null);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PostStopResponse>(content);
        
        if (result is { Success: false })
        {
            return null;
        }
        
        return result?.Data;
    }

    public static async Task<Dictionary<string, NalaiCoreInfo>?> SendDeleteMsgAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/download")
        {
            Query = $"id={id}"
        };
        var response = await HttpClient.DeleteAsync(uriBuilder.Uri);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<GetAllInfoResponse>(content);
        
        if (result is { Success: false })
        {
            return null;
        }
        
        return result?.Data;
    }
}