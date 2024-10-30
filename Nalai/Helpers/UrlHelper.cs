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
        
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Referer", url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(url));
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

        var fileNameFromUrl = url.Split('/').Last();
        return CleanFileName(fileNameFromUrl);
    }
    
    public static string CleanFileName(string fileName)
    {
        string invalidCharsPattern = "[<>:\"/\\\\|?*]";
        
        string cleanedFileName = Regex.Replace(fileName, invalidCharsPattern, "_");

        return cleanedFileName;
    }
}