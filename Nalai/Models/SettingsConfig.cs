using System;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Nalai.Helpers;
using Wpf.Ui.Appearance;

namespace Nalai.Models;

// 基类用于处理属性更改并自动保存配置
public class ObservableSaveConfig : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        Console.WriteLine(@"Setting Property changed: " + e.PropertyName);
        
        ConfigHelper.SaveConfig();
        ConfigHelper.ApplyConfig();
    }
}

// 所有的配置类
public partial class SettingsConfig : ObservableSaveConfig
{
    [ObservableProperty]
    private SettingGeneral _general = new();

    [ObservableProperty]
    private SettingAppearance _appearance = new();

    [ObservableProperty]
    private SettingDownload _download = new();
}

public partial class SettingGeneral : ObservableSaveConfig
{
    [ObservableProperty]
    private string _language = "en-US";

    [ObservableProperty]
    private bool _isStartWithWindows = true;

    [ObservableProperty]
    private bool _isStartMinimized = true;
}

public partial class SettingAppearance : ObservableSaveConfig
{
    [ObservableProperty]
    private ApplicationTheme _theme = ApplicationTheme.Dark;
}

public partial class SettingDownload : ObservableSaveConfig
{
    [ObservableProperty]
    private bool _isShowCompletedWindow = true;
}