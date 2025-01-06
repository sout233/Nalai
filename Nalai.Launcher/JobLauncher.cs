using System.Runtime.InteropServices;

namespace Nalai.Launcher;

public static class JobLauncher
{
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

    public static void LaunchExe(string exePath, string[] args, bool showWindow = true)
    {
        // Prepare the startup info
        var si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        // Control window visibility by setting wShowWindow
        si.wShowWindow = showWindow ? (short)5 : (short)0; // SW_SHOW (5) or SW_HIDE (0)
        // Set the STARTF_USESHOWWINDOW flag to indicate that wShowWindow is used
        si.dwFlags |= 0x00000001; // STARTF_USESHOWWINDOW
        
        var argsString = string.Join(" ", args);
        var argsPtr = Marshal.StringToHGlobalUni(argsString);

        // Set up the process information

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
            out var pi // Pointer to PROCESS_INFORMATION structure
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
}