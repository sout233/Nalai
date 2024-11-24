using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Nalai.Helpers;

public static class UrlHelper
{
    public static async Task<string?> GetTrueUrl(string url)
    {
        using var client = new HttpClient();
        // 设置HttpClient跟随重定向
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Referer", url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(url));
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var finalUrl = response.RequestMessage?.RequestUri;
            Console.WriteLine("Final URL: " + finalUrl);
            return finalUrl?.ToString();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error: {0}", e.Message);
        }
        
        return null;
    }
    
    public static async Task<string> GetFileName(string url)
    {
        using var client = new HttpClient();
    
        var uri = new Uri(url);
    
        // 构建 Referer 头部，只包含主机名和路径，不包含查询参数
        var referer = $"{uri.Scheme}://{uri.Host}{uri.PathAndQuery}";
    
        // 设置请求头
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Referer", referer);
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
    
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentDisposition != null &&
                !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
            {
                string fileName = response.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
                Console.WriteLine($"File name from server: {fileName}");
                return fileName;
            }

            Console.WriteLine("No file name provided by the server.");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }

        // 如果服务器没有提供文件名，则从 URL 中解析
        var fileNameFromUrl = uri.Segments.Last(); // 获取最后一个路径段落作为文件名
        return CleanFileName(fileNameFromUrl);
    }

    private static string CleanFileName(string fileName)
    {
        return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), "_"));
    }
}