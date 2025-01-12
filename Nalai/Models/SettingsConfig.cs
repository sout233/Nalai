using Wpf.Ui.Appearance;

namespace Nalai.Models;

public class SettingsConfig
{
    public SettingGeneral General { get; set; }
    public SettingAppearance Appearance { get; set; }
    public SettingDownload Download { get; set; }
}

public class SettingGeneral
{
    public string Language { get; set; }
    public bool isStartWithWindows { get; set; }
    public bool isUseSlientMode { get; set; }
}

public class SettingAppearance
{
    public ApplicationTheme Theme { get; set; }
}

public class SettingDownload
{
    public bool isShowCompletedWindow { get; set; }
}