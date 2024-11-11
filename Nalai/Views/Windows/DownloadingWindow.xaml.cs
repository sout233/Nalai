using Nalai.Models;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadingWindow : FluentWindow
{
    public DownloadingWindowViewModel ViewModel { get; }
    private CoreTask ThisWindowTask { get; set; }

    public DownloadingWindow(DownloadingWindowViewModel viewModel, string url, CoreTask task)
    {
        ViewModel = viewModel;
        DataContext = this;
        ThisWindowTask = task;

        InitializeComponent();
    }

    private void DownloadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var task = ThisWindowTask;
        var fileName = task.FileName;

        ViewModel.FileName = fileName;
        ViewModel.ApplicationTitle = "Downloading: " + fileName;
        ViewModel.ThisViewTask = task;
        ViewModel.BasedWindow = this;

        // TODO: 实现下载进度的监听（绑回去）
        task.ProgressChanged += ViewModel.OnDownloadProgressChanged;
        // 下面这个暂且不用
        // task.Downloader.ChunkDownloadProgressChanged += ViewModel.OnChunkDownloadProgressChanged;
    }

   
    // TODO: 不符合 MVVM 设计模式，需要重构
    private void ShowMoreBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ShowMoreVisibility == Visibility.Collapsed)
        {
            ViewModel.ShowMoreVisibility = Visibility.Visible;
            ViewModel.ShowMoreBtnContent = "Less";
            SymbolIcon symbolIcon = new()
            {
                Symbol = SymbolRegular.ChevronUp24
            };
            ViewModel.ShowMoreBtnIcon = symbolIcon;
            Height = 370 + 50;
        }
        else
        {
            ViewModel.ShowMoreVisibility = Visibility.Collapsed;
            ViewModel.ShowMoreBtnContent = "More";
            
            SymbolIcon symbolIcon = new()
            {
                Symbol = SymbolRegular.ChevronDown24
            };
            
            ViewModel.ShowMoreBtnIcon = symbolIcon;

            var height = TopStackPanel.Height + DashCardPanelGrid.Height + BottomPanelGrid.Height;
            Height = 370;
        }
    }
}