using System.Text;
using System.Text.Json;
using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.CoreConnector;

public class PreCore
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    public async Task<PostDownloadResult> StartAsync(string url, string path)
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
            var result = JsonConvert.DeserializeObject<PostDownloadResult>(contentResult);
            return result;
        }
        catch (HttpRequestException e)
        {
            // 处理异常
            Console.WriteLine(e.Message);
            return null;
        }
    }
    
    public async Task<GetStatusResult> GetStatusAsync(string id)
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
            var result = JsonConvert.DeserializeObject<GetStatusResult>(content);
            return result;
        }
        catch (HttpRequestException e)
        {
            // 处理异常
            Console.WriteLine(e.Message);
            return null;
        }
    }
}