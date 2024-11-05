namespace Nalai.CoreConnector;

public class PreCore
{
    public static async Task DownloadFile()
    {
    using (var client = new HttpClient()) {
        try
        {
            var response = await client.GetAsync("http://127.0.0.1:3030/download");
            response.EnsureSuccessStatusCode();
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream =
                   new FileStream("downloaded_file.zip", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }

            Console.WriteLine("下载完成！");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"请求错误: {e.Message}");
        }
    }

    }
    
}