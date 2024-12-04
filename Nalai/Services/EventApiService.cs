using System.IO;
using System.Net;
using Nalai.Helpers;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;
using Newtonsoft.Json;

namespace Nalai.Services;

public static class EventApiService
{
    public static event EventHandler<DownloadData> DownloadDataReceived;

    public static void Run()
    {
        const string url = "http://localhost:10389/download";
        var listener = new HttpListener();
        listener.Prefixes.Add(url + "/");
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);

        DownloadDataReceived += OnDownloadDataReceived;

        while (true)
        {
            // Wait for a connection
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            // Ensure the request is a POST and has content
            if (request.HttpMethod == "POST" && request.HasEntityBody)
            {
                using var body = request.InputStream;
                using var reader = new StreamReader(body, request.ContentEncoding);
                var jsonContent = reader.ReadToEnd();

                // Parse the JSON data
                try
                {
                    var data = JsonConvert.DeserializeObject<DownloadData>(jsonContent);

                    if (data != null)
                        DownloadDataReceived?.Invoke(null, data);
                    else
                        NalaiMsgBox.Show("Invalid data received from browser.");

                    // Respond to the client
                    const string responseString = "Data received successfully.";
                    var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    using var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Failed to parse JSON: " + ex.Message);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusDescription = "Invalid JSON format.";
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "Only POST method is allowed.";
            }

            // Close the response
            response.Close();
        }
    }

    private static void OnDownloadDataReceived(object? sender, DownloadData e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Console.WriteLine("Received data from browser: {0}, version: {1}, downloadUrl: {2}",
                e.Browser.Browser, e.Browser.Version, e.DownloadUrl);

            NewTaskWindowViewModel vm = new(e.DownloadUrl, string.Empty);
            NewTaskWindow window = new(vm);
            window.Show();
        });
    }

    public class BrowserInfo
    {
        [JsonProperty("browser")] public string Browser { get; set; }

        [JsonProperty("version")] public string Version { get; set; }
    }

    public class DownloadData
    {
        [JsonProperty("browser")] public BrowserInfo Browser { get; set; }

        [JsonProperty("userAgent")] public string UserAgent { get; set; }

        [JsonProperty("downloadUrl")] public string DownloadUrl { get; set; }
    }
}