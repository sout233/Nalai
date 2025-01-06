using System.IO;
using System.Net;
using Nalai.Helpers;
using Nalai.Models;
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
            
            // Handle preflight OPTIONS request
            if (request.HttpMethod == "OPTIONS")
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                continue;
            }
            
            // Handle root path GET request
            if (request is { HttpMethod: "GET", Url.AbsolutePath: "/" })
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.Headers.Add("Access-Control-Allow-Origin", "*");

                // Respond to the client
                const string responseString = "Root endpoint reached.";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                using var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                response.Close();
                continue;
            }


            // Ensure the request is a POST and has content
            if (request.HttpMethod == "POST" && request.HasEntityBody)
            {
                Console.WriteLine("Received data from browser.");
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

                    // Respond to the client with CORS headers
                    response.Headers.Add("Access-Control-Allow-Origin", "*");

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
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
            }
            else
            {
                Console.WriteLine("Invalid request received: {0} | {1}", request.HttpMethod, request.HasEntityBody);
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "Only POST method is allowed.";
                response.Headers.Add("Access-Control-Allow-Origin", "*");
            }

            // Close the response
            response.Close();
        }
    }

    public static void OnDownloadDataReceived(object? sender, DownloadData e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Console.WriteLine("Received data from browser: {0}, downloadUrl: {1}",
                e.Browser.Name, e.DownloadUrl);
            foreach (var header in e.Browser.Headers)
            {
                Console.WriteLine("Header: {0}: {1}", header.Key, header.Value);
            }

            NewTaskWindow window = new(e.DownloadUrl, string.Empty, e.Browser.Headers);
            window.Show();
        });
    }
}