using System.Windows.Interop;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Nalai.Helpers;

public class NalaiMsgBox
{
    public static void Show(string message,string title="Nalai")
    {
        Application.Current.Dispatcher.Invoke((Action)(() =>  
        {  
            MessageBox messageBox = new MessageBox();
            messageBox.Title = title;
            messageBox.Content = message;
            messageBox.ShowDialogAsync();
        }));  
    }
}