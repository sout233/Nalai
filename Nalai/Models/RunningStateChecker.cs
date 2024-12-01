using System.Net.Http;
using Nalai.CoreConnector.Models;
using Newtonsoft.Json;

namespace Nalai.Models;

public static class RunningStateChecker
{
        private static HealthStatus _status = HealthStatus.Unknown;

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
            StatusChanged?.Invoke(null, null);
        }

        public static event EventHandler<object> StatusChanged;
        //public HealthStatus Status;
        //private string _state;
        private static readonly System.Timers.Timer _timer = new(1500);
        private static readonly HttpClient _httpClient = new();
        
        private static async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs ea)
        {
            try
            {
                var uriBuilder = new UriBuilder("http://localhost:13088/checkhealth");
                
                HttpResponseMessage response = await _httpClient.GetAsync(uriBuilder.Uri);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                    Status = result.code.ToString() switch
                    {
                        "200 OK" => HealthStatus.Running,
                        _ => HealthStatus.Unknown,
                    };
                    Console.WriteLine($"Code: {result.code}");
                    Console.WriteLine($"Status: {Status}");
                }
                else
                {
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
        public static void ErrHandle(string e)
        {
            Console.WriteLine($"Error: {e}");
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
            Console.WriteLine("Stopped");
        }
}