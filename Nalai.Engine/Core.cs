// using System.ComponentModel;
// using System.Net.Http.Headers;
// using System.Timers;
// using Timer = System.Timers.Timer;
//
// namespace Nalai.Engine
// {
//     public sealed class Downloader : IDisposable
//     {
//         private readonly HttpClient _httpClient = new();
//         private CancellationTokenSource _cancellationTokenSource = new();
//         private bool _isDisposed;
//         private bool _isPaused;
//         private TaskCompletionSource<bool> _pauseTcs = new();
//         private int _chunkCount = 8; // 默认分块数量
//         private long _totalBytesRead;
//         private long _contentLength;
//         private readonly object _syncLock = new();
//         private Timer _speedTimer = new();
//         private long _previousTotalBytesRead;
//         private double _downloadSpeed;
//
//         public event EventHandler<ProgressChangedEventArgs> ProgressChanged = null!;
//         public event EventHandler DownloadCompleted = null!;
//         public event EventHandler<DownloadEvents.DownloadSpeedChangedEventArgs> DownloadSpeedChanged = null!;
//
//         private int _prevProgress;
//
//         public async Task DownloadFileAsync(string url, string outputPath)
//         {
//             try
//             {
//                 _httpClient.DefaultRequestHeaders.Add("Referer",
//                     url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(url));
//                 _httpClient.DefaultRequestHeaders.Add("Accept",
//                     "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
//                 _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
//                 _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
//                     "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
//
//                 using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead,
//                     _cancellationTokenSource.Token);
//
//                 response.EnsureSuccessStatusCode();
//
//                 if (response.Content.Headers.ContentLength == null)
//                 {
//                     throw new InvalidOperationException(
//                         "The server did not provide a content length for the requested resource.");
//                 }
//
//                 _contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();
//                 var chunkSize = (long)Math.Ceiling((double)_contentLength / _chunkCount);
//
//                 StartSpeedTimer();
//
//                 List<Task> tasks = new List<Task>();
//                 for (var i = 0; i < _chunkCount; i++)
//                 {
//                     var start = i * chunkSize;
//                     var end = Math.Min(start + chunkSize - 1, _contentLength - 1);
//                     tasks.Add(DownloadChunkAsync(url, outputPath, start, end, i));
//                 }
//
//                 await Task.WhenAll(tasks);
//
//                 Console.WriteLine("Download completed");
//
//                 OnDownloadCompleted(EventArgs.Empty);
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e);
//                 throw;
//             }
//         }
//
//         private async Task DownloadChunkAsync(string url, string outputPath, long start, long end, int chunkIndex)
//         {
//             using var request = new HttpRequestMessage(HttpMethod.Get, url);
//             request.Headers.Range = new RangeHeaderValue(start, end);
//
//             using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
//             response.EnsureSuccessStatusCode();
//
//             await using var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);
//             await DownloadFileFromStreamAsync(contentStream, outputPath, start, end, chunkIndex);
//         }
//
//         private async Task DownloadFileFromStreamAsync(Stream source, string outputPath, long start, long end, int chunkIndex)
//         {
//             await using var fileStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
//             fileStream.Seek(start, SeekOrigin.Begin);
//
//             var buffer = new byte[8192];
//             int bytesRead;
//             long totalBytesRead = 0;
//
//             while ((bytesRead = await source.ReadAsync(buffer, _cancellationTokenSource.Token)) > 0)
//             {
//                 if (_isPaused)
//                 {
//                     await _pauseTcs.Task;
//                     _pauseTcs = new TaskCompletionSource<bool>();
//                 }
//
//                 await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancellationTokenSource.Token);
//                 totalBytesRead += bytesRead;
//
//                 lock (_syncLock)
//                 {
//                     _totalBytesRead += bytesRead;
//                     var progress = (int)(_totalBytesRead * 100 / _contentLength);
//                     OnProgressChanged(new ProgressChangedEventArgs(progress, this));
//                 }
//             }
//         }
//
//         private async Task MergeChunksAsync(string outputPath, long contentLength)
//         {
//             var tempDir = Path.GetDirectoryName(outputPath)!;
//             var tempPrefix = Path.GetFileNameWithoutExtension(outputPath);
//             var tempExt = Path.GetExtension(outputPath);
//
//             using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
//             for (var i = 0; i < _chunkCount; i++)
//             {
//                 var tempFilePath = GetTempFilePath(outputPath, i);
//                 using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
//                 await fileStream.CopyToAsync(outputStream);
//                 File.Delete(tempFilePath);
//             }
//         }
//
//         private string GetTempFilePath(string outputPath, int chunkIndex)
//         {
//             var tempDir = Path.GetDirectoryName(outputPath)!;
//             var tempPrefix = Path.GetFileNameWithoutExtension(outputPath);
//             var tempExt = Path.GetExtension(outputPath);
//             return Path.Combine(tempDir, $"{tempPrefix}.part{chunkIndex}{tempExt}");
//         }
//
//         private void OnProgressChanged(ProgressChangedEventArgs e)
//         {
//             if (e.ProgressPercentage == _prevProgress) return;
//             ProgressChanged?.Invoke(this, e);
//             _prevProgress = e.ProgressPercentage;
//         }
//
//         private void OnDownloadCompleted(EventArgs e)
//         {
//             DownloadCompleted?.Invoke(this, e);
//         }
//
//         private void OnDownloadSpeedChanged(DownloadEvents.DownloadSpeedChangedEventArgs e)
//         {
//             DownloadSpeedChanged?.Invoke(this, e);
//         }
//
//         private void StartSpeedTimer()
//         {
//             _speedTimer = new Timer(1000);
//             _speedTimer.Elapsed += SpeedTimer_Elapsed;
//             _speedTimer.Start();
//         }
//
//         private void SpeedTimer_Elapsed(object? sender, ElapsedEventArgs e)
//         {
//             lock (_syncLock)
//             {
//                 var currentTotalBytesRead = _totalBytesRead;
//                 _downloadSpeed = (currentTotalBytesRead - _previousTotalBytesRead) / 1.0; // B/s
//                 _previousTotalBytesRead = currentTotalBytesRead;
//                 OnDownloadSpeedChanged(new DownloadEvents.DownloadSpeedChangedEventArgs(_downloadSpeed));
//             }
//         }
//
//         public void Pause()
//         {
//             if (_isPaused) return;
//             
//             _isPaused = true;
//             _cancellationTokenSource.Cancel();
//         }
//
//         public void Resume()
//         {
//             if (!_isPaused) return;
//             
//             _isPaused = false;
//             _cancellationTokenSource = new CancellationTokenSource();
//             _pauseTcs.SetResult(true); // send 恢复 的 sign
//         }
//
//         public void Stop()
//         {
//             _isPaused = false;
//             _cancellationTokenSource.Cancel();
//             _cancellationTokenSource.Dispose();
//             _cancellationTokenSource = new CancellationTokenSource();
//         }
//
//         public void Dispose()
//         {
//             Dispose(true);
//             GC.SuppressFinalize(this);
//         }
//
//         private void Dispose(bool disposing)
//         {
//             if (_isDisposed) return;
//             if (disposing)
//             {
//                 _httpClient.Dispose();
//                 _cancellationTokenSource.Dispose();
//                 _speedTimer?.Stop();
//                 _speedTimer?.Dispose();
//             }
//
//             _isDisposed = true;
//         }
//     }
// }