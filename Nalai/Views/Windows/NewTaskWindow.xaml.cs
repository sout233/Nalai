﻿using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class NewTaskWindow : FluentWindow
{
    public NewTaskWindowViewModel ViewModel { get; }

    private const string DownloadUrl =
        "https://download.fastmirror.net/download/Paper/1.21.1/build74";

    public NewTaskWindow(string url = "", string savePath = "", Dictionary<string, string>? headers = null)
    {
        InitializeComponent();
        DataContext = this;

        ViewModel = new NewTaskWindowViewModel(url, savePath)
        {
            Window = this,
            Headers = headers ?? new Dictionary<string, string>()
        };

        this.Closing += NewTaskWindow_Closing;
    }

    protected virtual void NewTaskWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 通知视图模型窗口即将关闭
        ViewModel.OnWindowClosing();
    }
}