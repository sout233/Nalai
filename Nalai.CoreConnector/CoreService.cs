﻿using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.CoreConnector;

public static class CoreService
{
    private static readonly HttpClient HttpClient = new();

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
        return result?.Data;
    }

    public static async Task<NalaiCoreStatus?> GetStatusAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/info")
        {
            Query = $"id={id}"
        };
        //HttpContent content = new();
        var response = await HttpClient.GetAsync(uriBuilder.Uri);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<GetStatusResponse>(content);

        if (result is { Success: false })
        {
            throw new Exception(result.Data.ToString());
        }
            
        return result?.Data;
    }
    
    public static async Task<NalaiSorcResult?> SendSorcMsgAsync(string id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/sorc");
        uriBuilder.Query = $"id={id}";
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
        var response = await HttpClient.PostAsync(uriBuilder.Uri, null);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PostSorcResponse>(content);
        return result?.Data;
    }

    public static async Task<NalaiStopResult?> SendStopMsgAsync(string? id)
    {
        var uriBuilder = new UriBuilder("http://localhost:13088/stop")
        {
            Query = $"id={id}"
        };
        Console.WriteLine($"uriBuilder.Uri:  {uriBuilder.Uri}");
        var response = await HttpClient.PostAsync(uriBuilder.Uri, null);
        response.EnsureSuccessStatusCode(); // 确保响应状态码为200-299
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PostStopResponse>(content);
        return result?.Data;
    }
}