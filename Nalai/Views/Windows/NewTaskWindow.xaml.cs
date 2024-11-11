using Nalai.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Nalai.Views.Windows;

public partial class NewTaskWindow : FluentWindow
{
    public NewTaskWindowViewModel ViewModel { get; }

    private const string DownloadUrl =
        "https://download.fastmirror.net/download/Paper/1.21.1/build74";

    public NewTaskWindow(NewTaskWindowViewModel vm, string url = "", string savePath = "")
    {
        DataContext = this;
        ViewModel = vm;
        ViewModel.Url = url;

        ViewModel.SavePath = savePath != ""
            ? savePath
            : System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads");

        ViewModel.Url = url != ""
            ? url
            : DownloadUrl;

        InitializeComponent();
    }
}
