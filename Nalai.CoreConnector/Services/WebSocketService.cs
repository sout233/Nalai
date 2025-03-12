using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.CoreConnector.Services;

public static class WebSocketService
{
    private const string WsUrl = "ws://127.0.0.1:13088/ws";
    private static readonly ClientWebSocket _client = new();
    public static event EventHandler<WsEvent<object>>? OnMessageReceived;

    public static async Task Start()
    {
        try
        {
            await _client.ConnectAsync(new Uri(WsUrl), CancellationToken.None);
            Console.WriteLine("Connected to server.");

            await ReceiveMessages(); // Use a separate method for receiving
        }
        catch (Exception ex)
        {
            Console.WriteLine("WS Error: " + ex.Message);
        }
    }

    private static async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 4]; // Start with a larger buffer (4KB)
        var receivedData = new StringBuilder();

        while (_client.State == WebSocketState.Open)
        {
            var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                receivedData.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                // Attempt to deserialize. If it fails, it might be an incomplete message.
                try
                {
                    var data = JsonConvert.DeserializeObject<WsEvent<object>>(receivedData.ToString());
                    if (data != null)
                    {
                        Console.WriteLine($"Received message: {receivedData}");
                        OnMessageReceived?.Invoke(null, data);
                        receivedData.Clear(); // Clear for the next message
                    }
                }
                catch (JsonReaderException)
                {
                    // Incomplete message, continue reading
                    Console.WriteLine("Incomplete JSON received, waiting for more data...");
                }
                catch (Exception ex)  // Catch other JSON deserialization exceptions
                {
                    Console.WriteLine($"JSON Deserialization Error: {ex}");
                    Console.WriteLine($"Received Data: {receivedData}"); // Log the received data for debugging
                    receivedData.Clear(); // Clear to prevent endless errors
                }

            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Connection closed by server.");
                break;
            }
        }

        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client is closing", CancellationToken.None);
        Console.WriteLine("Connection closed.");
    }



    public static async Task Close()
    {
        if (_client.State == WebSocketState.Open) { // Check if the connection is open
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client is closing", CancellationToken.None);
        }
        Console.WriteLine("WS Connection closed.");
    }
}