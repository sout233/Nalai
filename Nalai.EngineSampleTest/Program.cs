using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nalai.Engine;
using Nalai.Engine.Interfaces;
using Nalai.Engine.Models;
using Nalai.Engine.Services;

namespace Nalai.EngineSampleTest
{
    internal class Program
    {
        private static readonly CancellationTokenSource UserInputCancellationTokenSource = new();
        private static Downloader _downloader;
        
        private static async Task Main(string[] _)
        {
            IHttpClientProvider httpClientProvider = new HttpClientProvider();
            DownloaderConfiguration downloaderConfiguration = new();

            Downloader downloader = new(httpClientProvider, downloaderConfiguration);
            _downloader = downloader;

            Console.WriteLine("Press Enter to start downloading, 'p' to pause, 's' to stop, and 'r' to resume.");

            var keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine("Starting download...");

                const string url = "https://studygolang.com/dl/golang/go1.23.2.windows-amd64.msi";
                var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "go1.23.2.windows-amd64.msi");

                try
                {
                    downloader.ProgressChanged +=
                        (sender, e) => Console.WriteLine($"Progress: {e.ProgressPercentage}%");
                    downloader.DownloadCompleted += (sender, e) => Console.WriteLine("Download completed.");
                    downloader.DownloadSpeedChanged +=
                        (sender, e) => Console.WriteLine($"Download speed: {e.Speed} bytes/s");

                    Task.Run(() => ListenForKeyPresses(UserInputCancellationTokenSource.Token));

                    await downloader.DownloadFileAsync(url, outputPath);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Download was canceled by the user.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            UserInputCancellationTokenSource.Cancel();
            UserInputCancellationTokenSource.Dispose();
        }

        private static void ListenForKeyPresses(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.P:
                            Console.WriteLine("Pausing download...");
                            _downloader.Pause();
                            break;
                        case ConsoleKey.S:
                            Console.WriteLine("Stopping download...");
                            _downloader.Stop();
                            break;
                        case ConsoleKey.R:
                            Console.WriteLine("Resuming download...");
                            _downloader.Resume();
                            break;
                    }
                }
            }
        }
    }
}