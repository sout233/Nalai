using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nalai.Engine
{
    public class Downloader : IDisposable
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler DownloadCompleted;

        public Downloader()
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();

                using var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);

                await DownloadFileFromStreamAsync(contentStream, outputPath,
                    response.Content.Headers.ContentLength.GetValueOrDefault());
            }
            catch (OperationCanceledException) when (_cancellationTokenSource.IsCancellationRequested)
            {
                // Handle cancellation
            }
            catch (Exception ex)
            {
                // Handle other exceptions
            }
        }

        private async Task DownloadFileFromStreamAsync(Stream source, string outputPath,double length)
        {
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            byte[] buffer = new byte[8192];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
                totalBytesRead += bytesRead;
                OnProgressChanged(new ProgressChangedEventArgs((int)(totalBytesRead * 100 / length), this));
            }

            OnDownloadCompleted(EventArgs.Empty);
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnDownloadCompleted(EventArgs e)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                    _cancellationTokenSource.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}