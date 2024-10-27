using System.Windows.Controls;
using Downloader;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class DownloadingWindow : FluentWindow
{
    public DownloadingWindowViewModel ViewModel { get; }
    private DownloadTask ThisWindowTask { get; set; }

    public static DownloadingWindow CreateWindow(string url, DownloadTask task)
    {
        var viewModel = new DownloadingWindowViewModel();
        return new DownloadingWindow(viewModel, url, task);
    }

    public DownloadingWindow(DownloadingWindowViewModel viewModel, string url, DownloadTask task)
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

        task.Downloader.DownloadProgressChanged += ViewModel.OnDownloadProgressChanged;
        task.Downloader.ChunkDownloadProgressChanged += ViewModel.OnChunkDownloadProgressChanged;
    }

   

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