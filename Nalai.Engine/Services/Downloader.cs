using System.ComponentModel;
using System.Net.Http.Headers;
using System.Timers;
using Nalai.Engine.Helpers;
using Nalai.Engine.Interfaces;
using Nalai.Engine.Models;
using Timer = System.Timers.Timer;

namespace Nalai.Engine.Services
{
    public sealed class Downloader : IDownloader
    {
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly DownloaderConfiguration _config;
        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isDisposed;
        private bool _isPaused;
        private TaskCompletionSource<bool> _pauseTcs = new();
        private long _totalBytesRead;
        private long _contentLength;
        private readonly object _syncLock = new();
        private Timer _speedTimer = new();
        private long _previousTotalBytesRead;
        private double _downloadSpeed;
        private int _prevProgress;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged = null!;
        public event EventHandler DownloadCompleted = null!;
        public event EventHandler<DownloadEvents.DownloadSpeedChangedEventArgs> DownloadSpeedChanged = null!;

        public Downloader(IHttpClientProvider httpClientProvider, DownloaderConfiguration config)
        {
            _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                var client = _httpClientProvider.GetClient();
                client.DefaultRequestHeaders.Add("Referer",
                    url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(url));

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead,
                    _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength == null)
                {
                    throw new InvalidOperationException(
                        "The server did not provide a content length for the requested resource.");
                }

                _contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();
                var chunkSize = (long)Math.Ceiling((double)_contentLength / _config.ChunkCount);

                StartSpeedTimer();

                var semaphore = new SemaphoreSlim(_config.MaxConcurrentDownloads);

                var tasks = new List<Task>();
                for (var i = 0; i < _config.ChunkCount; i++)
                {
                    var start = i * chunkSize;
                    var end = Math.Min(start + chunkSize - 1, _contentLength - 1);
                    tasks.Add(DownloadChunkAsync(url, outputPath, start, end, i, semaphore));
                }

                await Task.WhenAll(tasks);

                await DownloadHelpers.MergeChunksAsync(outputPath, _contentLength, _config.ChunkCount);

                Console.WriteLine("Download completed");

                OnDownloadCompleted(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task DownloadChunkAsync(string url, string outputPath, long start, long end, int chunkIndex,
            SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync(_cancellationTokenSource.Token); // 等待信号量
            try
            {
                var client = _httpClientProvider.GetClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(start, end);

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();

                await using var contentStream =
                    await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);
                await DownloadFileFromStreamAsync(contentStream, outputPath, start, end, chunkIndex);
            }
            finally
            {
                semaphore.Release(); // 释放信号量
            }
        }

        private async Task DownloadFileFromStreamAsync(Stream source, string outputPath, long start, long end,
            int chunkIndex)
        {
            await using var fileStream = new FileStream(DownloadHelpers.GetTempFilePath(outputPath, chunkIndex),
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            fileStream.Seek(start, SeekOrigin.Begin);

            var buffer = new byte[8192];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await source.ReadAsync(buffer, _cancellationTokenSource.Token)) > 0)
            {
                if (_isPaused)
                {
                    await _pauseTcs.Task;
                    _pauseTcs = new TaskCompletionSource<bool>();
                }

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancellationTokenSource.Token);
                totalBytesRead += bytesRead;

                lock (_syncLock)
                {
                    _totalBytesRead += bytesRead;
                    var progress = (int)(_totalBytesRead * 100 / _contentLength);
                    if (progress != _prevProgress) // 平滑进度更新
                    {
                        OnProgressChanged(new ProgressChangedEventArgs(progress, this));
                        _prevProgress = progress;
                    }
                }
            }
        }

        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == _prevProgress) return;
            ProgressChanged?.Invoke(this, e);
            _prevProgress = e.ProgressPercentage;
        }

        private void OnDownloadCompleted(EventArgs e)
        {
            DownloadCompleted?.Invoke(this, e);
        }

        private void OnDownloadSpeedChanged(DownloadEvents.DownloadSpeedChangedEventArgs e)
        {
            DownloadSpeedChanged?.Invoke(this, e);
        }

        private void StartSpeedTimer()
        {
            _speedTimer = new Timer(1000);
            _speedTimer.Elapsed += SpeedTimer_Elapsed;
            _speedTimer.Start();
        }

        private void SpeedTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_syncLock)
            {
                var currentTotalBytesRead = _totalBytesRead;
                _downloadSpeed = (currentTotalBytesRead - _previousTotalBytesRead) / 1.0; // B/s
                _previousTotalBytesRead = currentTotalBytesRead;
                OnDownloadSpeedChanged(new DownloadEvents.DownloadSpeedChangedEventArgs(_downloadSpeed));
            }
        }

        public void Pause()
        {
            if (_isPaused) return;

            _isPaused = true;
            _cancellationTokenSource.Cancel();
        }

        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _cancellationTokenSource = new CancellationTokenSource();
            _pauseTcs.SetResult(true); // 发送恢复的信号
        }

        public void Stop()
        {
            _isPaused = false;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                _speedTimer?.Stop();
                _speedTimer?.Dispose();
            }

            _isDisposed = true;
        }
    }
}