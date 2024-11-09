using System.Text.Json;

namespace Nalai.CoreConnector;

public class PreCore
{
    private static readonly HttpClient Client = new HttpClient();

    static async Task Download()
    {
        // 发起下载请求
        string downloadUrl = "http://127.0.0.1:10388/download?url=https://mirrors.tuna.tsinghua.edu.cn/debian/dists/Debian10.13/ChangeLog&save_dir=C:/download";
        HttpResponseMessage response = await Client.GetAsync(downloadUrl);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        // 解析返回的JSON以获取ID
        var idJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        string id = idJson.GetProperty("id").GetString();

        Console.WriteLine($"Download initiated with ID: {id}");

        // 检查下载状态
        string statusUrl = $"http://127.0.0.1:13088/status?id={id}";
        while (true)
        {
            try
            {
                response = await Client.GetAsync(statusUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                var statusJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
                int downloadedBytes = statusJson.GetProperty("downloaded_bytes").GetInt32();
                int totalSize = statusJson.GetProperty("total_size").GetInt32();
                string fileName = statusJson.GetProperty("file_name").GetString();
                string url = statusJson.GetProperty("url").GetString();
                string status = statusJson.GetProperty("status").GetString();

                Console.WriteLine($"File: {fileName}, URL: {url}, Status: {status}, Downloaded: {downloadedBytes}/{totalSize} bytes");

                if (status == "Finished" || status == "Failed")
                {
                    break; // 如果下载完成或者失败，则停止循环
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking status: {ex.Message}");
                break;
            }

            await Task.Delay(200); // 每隔200毫秒检查一次
        }
    }
}