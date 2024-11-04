using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nalai.Engine;

namespace Nalai.EngineSampleTest
{
    internal class Program
    {
        private static readonly Downloader Downloader = new();
        private static readonly CancellationTokenSource UserInputCancellationTokenSource = new();

        private static async Task Main(string[] _)
        {
            Console.WriteLine("Press Enter to start downloading, 'p' to pause, 's' to stop, and 'r' to resume.");

            // 先等待用户按下Enter键
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine("Starting download...");

                const string url = "https://go.dev/dl/go1.23.2.windows-amd64.msi"; // 修改了URL
                var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "go1.23.2.windows-amd64.msi");

                try
                {
                    Downloader.ProgressChanged += (sender, e) => Console.WriteLine($"Progress: {e.ProgressPercentage}%");
                    Downloader.DownloadCompleted += (sender, e) => Console.WriteLine("Download completed.");

                    // 开始监听按键输入
                    Task.Run(() => ListenForKeyPresses(UserInputCancellationTokenSource.Token));

                    await Downloader.DownloadFileAsync(url, outputPath);
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

            UserInputCancellationTokenSource.Cancel(); // 正确使用 Cancel 方法
            UserInputCancellationTokenSource.Dispose();
        }

        private static void ListenForKeyPresses(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true); // 不显示按键
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.P:
                            Console.WriteLine("Pausing download...");
                            Downloader.Pause();
                            break;
                        case ConsoleKey.S:
                            Console.WriteLine("Stopping download...");
                            Downloader.Stop();
                            break;
                        case ConsoleKey.R:
                            Console.WriteLine("Resuming download...");
                            Downloader.Resume();
                            break;
                    }
                }
            }
        }
    }
}