using System.Diagnostics;
using System.Net.Http;
using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.Models;

public static class RunningStateChecker
{
        private static HealthStatus _status = HealthStatus.Unknown;
        private static long _runningTime;

        public static long RunningTime
        {
            get => _runningTime;
            set { _runningTime = value; OnTimeChanged(); }
        }

        public class TimeChangedEventArgs(long timestamp) : EventArgs
        {
            public long TimeStamp { get; } = timestamp;
        }

        public class StatusChangedEventArgs(HealthStatus status) : EventArgs
        {
            public HealthStatus Status { get; } = status;
        }

        public static HealthStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnStatusChanged();
            }
        }

        private static void OnStatusChanged()
        {
            // 触发事件
            StatusChanged?.Invoke(null, new StatusChangedEventArgs(Status));
        }

        private static void OnTimeChanged()
        {
            TimeChanged?.Invoke(null,new TimeChangedEventArgs(RunningTime));
        }
        public static event EventHandler<TimeChangedEventArgs> TimeChanged;
        public static event EventHandler<StatusChangedEventArgs> StatusChanged;
        //public HealthStatus Status;
        //private string _state;
        private static readonly System.Timers.Timer _timer = new(1500);
        private static readonly HttpClient _httpClient = new();
        
        private static async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs ea)
        {
            try
            {
                var uriBuilder = new UriBuilder("http://localhost:13088/checkhealth");
                var cts = new CancellationTokenSource(); // 创建一个CancellationTokenSource
                cts.CancelAfter(500);
                using var response = await _httpClient.GetAsync(uriBuilder.Uri, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                    Status = result?.code.ToString() switch
                    {
                        "200" => HealthStatus.Running,
                        _ => HealthStatus.Unknown,
                    };
                    RunningTime++;
                    //Add 1
                    Debug.WriteLine($"Code: {result?.code}");
                    Debug.WriteLine($"Status: {Status}");
                }
                else
                {
                    ErrHandle($"Error: {response.StatusCode}");
                }
            }
            catch (OperationCanceledException)
            {
                // 处理请求被取消的情况
                Status = HealthStatus.Caution;
                Debug.WriteLine("Request timed out, status set to Caution");
            }
            catch (Exception exc)
            {
                // 处理异常
                Console.WriteLine(exc.Message);
                ErrHandle(exc.Message);
            }

            // Console.WriteLine(Status);
        }

        private static void ErrHandle(string e)
        {
            Debug.WriteLine($"Error: {e}");
            Status = HealthStatus.Unknown;
        }
        public static void Check()
        {
            Timer_Elapsed(null,null);
        }

        public static void Start()
        {
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

        }

        public static void Stop()
        {
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Enabled = false;
            _httpClient.Dispose();
            Debug.WriteLine("Stopped");
        }
}