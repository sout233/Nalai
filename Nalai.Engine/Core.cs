using System.ComponentModel;

namespace Nalai.Engine
{
    public sealed class Downloader : IDisposable
    {
        private readonly HttpClient _httpClient = new();
        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isDisposed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged = null!;
        public event EventHandler DownloadCompleted = null!;
        
        private int prevProgress = 0;

        public async Task DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Referer",
                    url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(url));
                _httpClient.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead,
                    _cancellationTokenSource.Token);

                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength == null)
                {
                    throw new InvalidOperationException(
                        "The server did not provide a content length for the requested resource.");
                }

                await using var contentStream =
                    await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);

                await DownloadFileFromStreamAsync(contentStream, outputPath,
                    response.Content.Headers.ContentLength.GetValueOrDefault());

                Console.WriteLine("Download completed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task DownloadFileFromStreamAsync(Stream source, string outputPath,double length)
        {
            await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[8192];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await source.ReadAsync(buffer, _cancellationTokenSource.Token)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancellationTokenSource.Token);
                totalBytesRead += bytesRead;
                OnProgressChanged(new ProgressChangedEventArgs((int)(totalBytesRead * 100 / length), this));
            }

            OnDownloadCompleted(EventArgs.Empty);
        }

        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == prevProgress) return;
            ProgressChanged?.Invoke(this, e);
            prevProgress = e.ProgressPercentage;
        }

        private void OnDownloadCompleted(EventArgs e)
        {
            DownloadCompleted?.Invoke(this, e);
        }

        public void Pause()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Resume()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        public void Stop()
        {
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
                _httpClient.Dispose();
                _cancellationTokenSource.Dispose();
            }

            _isDisposed = true;
        }
    }
}