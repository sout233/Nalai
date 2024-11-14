using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.Models
{
    public class CoreTask(string url, string savePath)
    {
        public NalaiCoreStatus StatusResult
        {
            get => _statusResult;
            private set
            {
                _statusResult = value;
                StatusText = value.StatusText;
                FileName = value.FileName;
                Url = value.Url;
                GlobalTaskChanged?.Invoke(this, this);
            }
        }

        public string FileName { get; set; } = "Unknown";
        public string SavePath { get; set; } = savePath;
        public string Url { get; set; } = url;
        public string? Id { get; set; }
        public string StatusText { get; set; } = "Pending";

        public List<Window> BindWindows { get; set; } = [];

        public event EventHandler<NalaiCoreStatus>? StatusChanged;
        public event EventHandler<DownloadProgressChangedEventArgs>? ProgressChanged;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private NalaiCoreStatus _statusResult = new();

        public static event EventHandler<CoreTask>? GlobalTaskChanged;
        
        public async Task StartDownloadAsync()
        {
            var result = await CoreService.SendStartMsgAsync(Url, SavePath);
            Id = result?.Id;
            StartListen(_cancellationTokenSource.Token);
        }

        public async Task StopAsync()
        {
            if (Id != null)
            {
                await CoreService.SendStopMsgAsync(Id);
            }

            await _cancellationTokenSource.CancelAsync();

            Console.WriteLine("StopAsync:" + Id);

            var info = await CoreService.GetStatusAsync(Id);
            if (info != null) StatusResult = info;
        }

        private void StartListen(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested && StatusResult?.StatusText != "Finished")
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var result = await CoreService.GetStatusAsync(Id);

                        if (result != null)
                        {
                            if (result.StatusText != StatusResult?.StatusText || result.FileName != FileName ||
                                result.Url != Url)
                            {
                                Console.WriteLine("Status changed:" + FileName);
                                if (StatusResult != null) StatusChanged?.Invoke(this, StatusResult);
                            }

                            if (result.DownloadedBytes != StatusResult?.DownloadedBytes)
                            {
                                ProgressChanged?.Invoke(this,
                                    new DownloadProgressChangedEventArgs(totalBytesToReceive: result.TotalSize,
                                        bytesReceived: result.DownloadedBytes,
                                        progressPercentage: (float)result.DownloadedBytes / result.TotalSize * 100,
                                        bytesPerSecondSpeed: result.BytesPerSecondSpeed)
                                );
                            }

                            StatusResult = result;
                            GlobalTaskChanged?.Invoke(this, this);
                        }

                        if (StatusResult != null)
                        {
                            FileName = StatusResult.FileName;
                            Url = StatusResult.Url;
                            StatusText = StatusResult.StatusText;

                            if (StatusResult.Status is DownloadStatus.Finished or DownloadStatus.Error)
                            {
                                Console.WriteLine("Download End");

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var vm = new DownloadCompleteWindowViewModel(this);
                                    var downloadCompleteWindow = new DownloadCompleteWindow(vm, this);
                                    vm.BindWindow = downloadCompleteWindow;
                                    downloadCompleteWindow.Show();

                                    foreach (var window in BindWindows)
                                    {
                                        if (window is DownloadingWindow downloadingWindow)
                                        {
                                            Console.WriteLine(
                                                $"Closing window: {downloadingWindow.ViewModel.ApplicationTitle}");
                                            downloadingWindow.Close();
                                        }
                                    }
                                });

                                await _cancellationTokenSource.CancelAsync();

                                break;
                            }

                            if (StatusResult.Status is DownloadStatus.Cancelled)
                            {
                                await _cancellationTokenSource.CancelAsync();

                                throw new OperationCanceledException();
                            }
                        }

                        // Console.WriteLine(
                        //     $"File: {FileName}, Status: {StatusResult?.StatusText}, Downloaded: {StatusResult?.DownloadedBytes} / {StatusResult?.TotalSize}, CToken: {cancellationToken.IsCancellationRequested}");

                        await Task.Delay(100, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Listen canceled");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }, cancellationToken);
        }
    }
}