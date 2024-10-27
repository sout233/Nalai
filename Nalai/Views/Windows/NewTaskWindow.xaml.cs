using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class NewTaskWindow : FluentWindow
{
    public NewTaskWindowViewModel ViewModel { get; }

    private const string DownloadUrl =
        "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

    public NewTaskWindow(NewTaskWindowViewModel vm, string url = "", string savePath = "")
    {
        DataContext = this;
        ViewModel = vm;
        ViewModel.Url = url;

        ViewModel.SavePath = savePath != ""
            ? savePath
            : System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads");

        ViewModel.Url = url != ""
            ? url
            : DownloadUrl;

        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public static class DialogCloser
{
    public static readonly DependencyProperty DialogResultProperty =
        DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool?),
            typeof(DialogCloser),
            new PropertyMetadata(DialogResultChanged));

    private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = d as Window;
        if (window != null)
        {
            window.DialogResult = e.NewValue as bool?;
        }
    }

    public static void SetDialogResult(Window target, bool? value)
    {
        target.SetValue(DialogResultProperty, value);
    }
}