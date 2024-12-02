using System.Windows.Interop;
using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;
using Nalai.Helpers;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.Models;

public class CoreTask(string url, string saveDir, string fileName, string id)
{
    private NalaiCoreInfo InfoResult
    {
        get => _infoResult;
        set
        {
            _infoResult = value;
            FileName = value.FileName;
            Url = value.Url;
            SaveDir = value.SaveDirectory;
            Chunks = value.Chunks.Select(chunk => new ExtendedChunkItem(chunk)).ToList();

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


    #region 属性和它的朋友们

    public string FileName { get; set; } = "Unknown";
    public string SaveDir { get; set; } = saveDir;
    public string Url { get; set; } = url;
    public string Id { get; set; } = id;

    // Text属性是为了方便绑定到界面上
    public DownloadStatus Status => InfoResult.Status;
    public string RealtimeStatusText { get; set; } = "Pending";


    public long TotalBytes => InfoResult.TotalBytes;
    public string TotalSizeText => ByteSizeFormatter.FormatSize(TotalBytes);


    public long DownloadedBytes => InfoResult.DownloadedBytes;
    public string DownloadedSizeText => ByteSizeFormatter.FormatSize(DownloadedBytes);

    public long BytesPerSecondSpeed => InfoResult.BytesPerSecondSpeed;
    public string SpeedText => ByteSizeFormatter.FormatSize(BytesPerSecondSpeed) + "/s";

    public float Progress => (float)DownloadedBytes / TotalBytes * 100;

    private RustSystemTime RawCreatedTime => InfoResult.CreatedTime;
    public string CreatedTimeText => TimeFormatter.FormatRustSystemTime(RawCreatedTime);
    public DateTimeOffset SortableCreatedTime => TimeFormatter.ConvertRustSystemTime(RawCreatedTime);

    public TimeSpan Eta => TimeFormatter.CalculateRemainingTime(DownloadedBytes, TotalBytes, BytesPerSecondSpeed);
    public string EtaText => TimeFormatter.FormatTimeSpanReadable(Eta);

    public List<ExtendedChunkItem> Chunks{ get; set; }

    #endregion

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
            var result = await CoreService.SendStartMsgAsync(Url, SaveDir, fileName,
                CalculateNalaiCoreId.FromFileNameAndSaveDir(fileName, saveDir));
            Id = result?.Id;
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        StartListen(_cancellationTokenSource.Token);
        NalaiDownService.ListeningTasks.TryAdd(Id, this);

        await Task.Run(SyncAllTasksFromCore);
    }

    public static void SyncAllTasksFromCore()
    {
        try
        {
            var infos = CoreService.GetAllInfo();
            if (infos != null)
            {
                var tempTasks = new Dictionary<string, CoreTask>();
                foreach (var (id, info) in infos)
                {
                    if (NalaiDownService.ListeningTasks.ContainsKey(id))
                    {
                        tempTasks.Add(id, NalaiDownService.ListeningTasks[id]!);
                        continue;
                    }

                    var task = new CoreTask(info.Url, info.SaveDirectory, info.FileName, id);

                    task.SetInfoResult(info);
                    task.Id = id;

                    tempTasks.Add(id, task);
                    Console.WriteLine($"New Task: {id}");
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
        try
        {
            await CoreService.SendStopMsgAsync(Id);
        }
        catch (Exception ex)
        {
            NalaiMsgBox.Show(ex.Message, "Error");
            return;
        }

        await _cancellationTokenSource.CancelAsync();

        Console.WriteLine("StopAsync:" + Id);

        var info = await CoreService.GetStatusAsync(Id);
        if (info != null) InfoResult = info;
        
        NalaiDownService.ListeningTasks.Remove(Id);
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

            NalaiDownService.ListeningTasks.Remove(Id);
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
        NalaiDownService.ListeningTasks.TryAdd(Id, this);

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
                        
                        Console.WriteLine(
                            $"File: {FileName}, Status: {RealtimeStatusText}, Downloaded: {InfoResult?.DownloadedBytes} / {InfoResult?.TotalBytes}");

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
                                        var handle = new WindowInteropHelper(downloadingWindow).Handle;
                                        if (handle != IntPtr.Zero)
                                        {
                                            downloadingWindow.Visibility = Visibility.Collapsed;
                                            BindWindows.Remove(downloadingWindow);
                                        }
                                    }
                                }
                            });
                            
                            SyncAllTasksFromCore();
                            
                            await _cancellationTokenSource.CancelAsync();

                            GlobalTaskChanged?.Invoke(this, this);

                            break;
                        }

                        if (InfoResult.Status is DownloadStatus.Cancelled)
                        {
                            SyncAllTasksFromCore();
                            
                            await _cancellationTokenSource.CancelAsync();
                            
                            GlobalTaskChanged?.Invoke(this, this);

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
                NalaiDownService.ListeningTasks.Remove(Id);
                Console.WriteLine("Listen canceled");
            }
            catch (Exception ex)
            {
                NalaiDownService.ListeningTasks.Remove(Id);
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
            
            NalaiDownService.ListeningTasks.Remove(Id);
        }

        return result is { IsRunning: true };
    }
}