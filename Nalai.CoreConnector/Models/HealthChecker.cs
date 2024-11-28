using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;


public class HealthChecker
{
    public delegate void StatusChangedEventHandler(object sender, EventArgs e);

    public event StatusChangedEventHandler StatusChanged ;
    public HealthStatus Status;
    private string _state;
    private readonly System.Timers.Timer _timer = new(1500);
    private readonly HttpClient _httpClient = new();
    public string StateText
    {
        get => _state;
        set
        {
            _state = value;
            Status = value switch
            {
                "200 OK" => HealthStatus.Running,
                _ => HealthStatus.Unknown,
            };
            OnStatusChanged();
            
        }
    }

    

    protected virtual void OnStatusChanged()
    {
        StatusChanged?.Invoke(this,EventArgs.Empty);
    }
    private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs ea)
    {
        try
        {
            var uriBuilder = new UriBuilder("http://localhost:13088/checkhealth");

            // 使用 _httpClient 发送异步HTTP请求
            HttpResponseMessage response = await _httpClient.GetAsync(uriBuilder.Uri);

            // 确保响应状态为成功
            if (response.IsSuccessStatusCode)
            {
                string jsonContent = await response.Content.ReadAsStringAsync();

                // 解析JSON内容
                var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                // 或者，如果您知道JSON结构，可以将其解析为特定的C#对象
                // var result = JsonConvert.DeserializeObject<YourClass>(jsonContent);

                // 处理解析后的对象
                
                StateText = result.code;
                Console.WriteLine(result.code);
                Console.WriteLine(Status);
            }
            else
            {
                // 处理错误响应
                ErrHandle($"Error: {response.StatusCode}");
            }
        }
        catch (Exception exc)
        {
            // 处理异常
            Console.WriteLine(exc.Message);
            ErrHandle(exc.Message);
        }
        Console.WriteLine(Status);
    }

    public void ErrHandle(string e)
    {
        Console.WriteLine($"Error: {e}");
        Status = HealthStatus.Unknown;
    }
    public void Start()
    {
        _timer.Elapsed += Timer_Elapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;

    }

    public void Stop()
    {
        _timer.Elapsed -= Timer_Elapsed;
        _timer.Enabled = false;
        _httpClient.Dispose();
        Console.WriteLine("Stopped");
    }
}
