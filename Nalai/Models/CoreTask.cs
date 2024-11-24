using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.Models;

public class CoreTask(string url, string savePath)
{
    public NalaiCoreInfo InfoResult
    {
        get => _infoResult;
        private set
        {
            _infoResult = value;
            FileName = value.FileName;
            Url = value.Url;
            SavePath = value.SaveDirectory;

            var totalFileSize = ByteSizeFormatter.FormatSize(value.TotalBytes);
            var receivedFileSize = ByteSizeFormatter.FormatSize(value.DownloadedBytes);

            FileSizeText = $"{receivedFileSize} / {totalFileSize}";

            if (value.Status == DownloadStatus.Running)
            {
                var progress = (float)value.DownloadedBytes / value.TotalBytes * 100;
                RealtimeStatusText = value.StatusText + $" ({progress:0.00}%)";
            }
            else
            {
                RealtimeStatusText = value.StatusText;
            }

            GlobalTaskChanged?.Invoke(this, this);
            StatusChanged?.Invoke(this, value);
        }
    }

    public string FileName { get; set; } = "Unknown";
    public string SavePath { get; set; } = savePath;
    public string Url { get; set; } = url;
    public string? Id { get; set; }
    public string RealtimeStatusText { get; set; } = "Pending";
    public string FileSizeText { get; set; } = "Unknown";

    public List<Window> BindWindows { get; set; } = [];

    public event EventHandler<NalaiCoreInfo>? StatusChanged;
    public event EventHandler<DownloadProgressChangedEventArgs>? ProgressChanged;

    private CancellationTokenSource _cancellationTokenSource = new();
    private NalaiCoreInfo _infoResult = new();

    public static event EventHandler<CoreTask>? GlobalTaskChanged;

    public async Task StartDownloadAsync()
    {
        try
        {
            var result = await CoreService.SendStartMsgAsync(Url, SavePath);
            Id = result?.Id;
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        StartListen(_cancellationTokenSource.Token);
    }

    public static void SyncAllTasksFromCore()
    {
        try
        {
            var infos = CoreService.GetAllInfo();
            if (infos != null)
            {
                var tempTasks = new List<CoreTask>();
                foreach (var info in infos)
                {
                    var task = new CoreTask(info.Value.Url, info.Value.SaveDirectory);

                    task.SetInfoResult(info.Value);
                    task.Id = info.Key;

                    tempTasks.Add(task);
                }

                NalaiDownService.GlobalDownloadTasks = tempTasks!;
                GlobalTaskChanged?.Invoke(null, null);
            }
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
        }
    }


    private void SetInfoResult(NalaiCoreInfo info)
    {
        InfoResult = info;
    }

    public async Task FakePauseAsync()
    {
        await InnerStopAsync();
    }

    public async Task CancelAsync()
    {
        await InnerStopAsync();
        CloseAllBindWindows();
    }

    private void CloseAllBindWindows()
    {
        foreach (var window in BindWindows)
        {
            window.Close();
        }
    }

    private async Task InnerStopAsync()
    {
        if (Id != null)
        {
            try
            {
                await CoreService.SendStopMsgAsync(Id);
            }
            catch (Exception ex)
            {
                NalaiMsgBox.Show(ex.Message, "Error");
                return;
            }
        }

        await _cancellationTokenSource.CancelAsync();

        Console.WriteLine("StopAsync:" + Id);

        var info = await CoreService.GetStatusAsync(Id);
        if (info != null) InfoResult = info;
    }

    public async Task DeleteAsync()
    {
        try
        {
            var result = await CoreService.SendDeleteMsgAsync(Id);

            if (result != null)
            {
                await Task.Run(SyncAllTasksFromCore);
            }

            CloseAllBindWindows();
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
        }
    }

    private void StartListen(CancellationToken cancellationToken)
    {
        Console.WriteLine("StartListen:" + Id);
        Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && InfoResult?.StatusText != "Finished")
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = await CoreService.GetStatusAsync(Id);

                    // 用于更新Status
                    if (result != null)
                    {
                        if (result.StatusText != InfoResult?.StatusText || result.FileName != FileName ||
                            result.Url != Url)
                        {
                            Console.WriteLine("Status changed:" + FileName);
                            if (InfoResult != null)
                                StatusChanged?.Invoke(this, InfoResult);
                        }

                        if (result.DownloadedBytes != InfoResult?.DownloadedBytes)
                        {
                            ProgressChanged?.Invoke(this,
                                new DownloadProgressChangedEventArgs(totalBytesToReceive: result.TotalBytes,
                                    bytesReceived: result.DownloadedBytes,
                                    progressPercentage: (float)result.DownloadedBytes / result.TotalBytes * 100,
                                    bytesPerSecondSpeed: result.BytesPerSecondSpeed)
                            );
                        }

                        InfoResult = result;
                        GlobalTaskChanged?.Invoke(this, this);
                    }

                    // 取消与停止的验证
                    if (InfoResult != null)
                    {
                        if (InfoResult.Status is DownloadStatus.Finished or DownloadStatus.Error)
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

                        if (InfoResult.Status is DownloadStatus.Cancelled)
                        {
                            await _cancellationTokenSource.CancelAsync();

                            throw new OperationCanceledException();
                        }
                    }

                    // Console.WriteLine(
                    //     $"File: {FileName}, Status: {InfoResult?.RealtimeStatusText}, Downloaded: {InfoResult?.DownloadedBytes} / {InfoResult?.TotalBytes}, CToken: {cancellationToken.IsCancellationRequested}");

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

    public async Task<bool> StartOrCancelAsync()
    {
        if (Id == null) return false;
        var result = await CoreService.SendSorcMsgAsync(Id);

        if (result is { IsRunning: true })
        {
            _cancellationTokenSource = new();
            StartListen(_cancellationTokenSource.Token);
        }
        else
        {
            await _cancellationTokenSource.CancelAsync();
            InfoResult = InfoResult with { StatusText = "Cancelled" };
            GlobalTaskChanged?.Invoke(this, this);
            StatusChanged?.Invoke(this, InfoResult);
        }

        return result is { IsRunning: true };
    }
}