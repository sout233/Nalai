using System.Net.Http;

namespace Nalai.Helpers;

public static class UrlHelper
{
    public static async Task<string?> GetTrueUrl(string url)
    {
        using var client = new HttpClient();
        // 设置HttpClient跟随重定向
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

        try
        {
            using var response = await client.GetAsync("http://example.com/redirected-link", HttpCompletionOption.ResponseHeadersRead);

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
    
    public static string GetFileName(string url)
    {
        string[] parts = url.Split('/');
        return parts[^1];
    }
}