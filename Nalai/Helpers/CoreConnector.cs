using System.Net.Http;
using System.Net.Http.Headers;

namespace Nalai.Helpers;

public class CoreConnector
{
    //POST localhost:13088/download?url=...&save_dir=...
    //returns unique id
    //GET localhost:13088/status?id=
    private static readonly HttpClient _httpClient = new HttpClient();
    public async Task<string> StartAsync(string url, string path)
    {
        try
        {
            var uriBuilder = new UriBuilder("http://localhost:13088/download");
            uriBuilder.Query = $"url={url}&save_dir={path}";
            //HttpContent content = new();
            Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
            var response = await _httpClient.PostAsync(uriBuilder.Uri,null);
            response.EnsureSuccessStatusCode();
            var contentResult = await response.Content.ReadAsStringAsync();
            return contentResult;
        }
        catch (HttpRequestException e)
        {
            // 处理异常
            Console.WriteLine(e.Message);
            return null;
        }
    }
    public async Task<string> GetStatusAsync(string id)
    {
        try
        {
            var uriBuilder = new UriBuilder("http://localhost:13088/status");
            uriBuilder.Query = $"id={id}";
            //HttpContent content = new();
            Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
            HttpResponseMessage response = await _httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (HttpRequestException e)
        {
            // 处理异常
            Console.WriteLine(e.Message);
            return null;
        }
    }
}