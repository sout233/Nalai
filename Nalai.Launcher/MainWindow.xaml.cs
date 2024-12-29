using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace Nalai.Launcher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Define the CREATE_BREAKAWAY_FROM_JOB flag
    private const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;

    // Import the CreateProcess function from kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcess(
        string? lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    // Define the STARTUPINFO and PROCESS_INFORMATION structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct STARTUPINFO
    {
        public Int32 cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public Int32 dwX;
        public Int32 dwY;
        public Int32 dwXSize;
        public Int32 dwYSize;
        public Int32 dwXCountChars;
        public Int32 dwYCountChars;
        public Int32 dwFillAttribute;
        public Int32 dwFlags;
        public Int16 wShowWindow;
        public Int16 cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 dwProcessId;
        public Int32 dwThreadId;
    }

    private static void LaunchNalai(string[] args)
    {
        // Prepare the startup info
        var si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(si);

        // Set up the process information
        var pi = new PROCESS_INFORMATION();

        // Construct the path to the executable
        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nalai.exe");

        // Call CreateProcess with the CREATE_BREAKAWAY_FROM_JOB flag
        var success = CreateProcess(
            null, // No module name (use command line)
            exePath, // Command line
            IntPtr.Zero, // Process handle not inheritable
            IntPtr.Zero, // Thread handle not inheritable
            false, // Set handle inheritance to FALSE
            CREATE_BREAKAWAY_FROM_JOB, // Creation flags
            IntPtr.Zero, // Use parent's environment block
            null, // Use parent's starting directory 
            ref si, // Pointer to STARTUPINFO structure
            out pi // Pointer to PROCESS_INFORMATION structure
        );

        if (!success)
        {
            Console.WriteLine($"Failed to create process. Error: {Marshal.GetLastWin32Error()}");
            return;
        }

        // Close process and thread handles
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        LaunchNalai([]);

        // 退出应用程序
        Application.Current.Shutdown();
    }
}