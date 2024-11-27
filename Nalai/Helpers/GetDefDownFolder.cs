using System.Runtime.InteropServices;

namespace Nalai.Helpers;

public static class GetFolderDefault
{
    private static class KnownFolder
    {
        public static readonly Guid Downloads = new("374DE290-123F-4565-9164-39C4925E467B");
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

    public static string GetDownloadPath()
    {
        // TODO: Handle errors
        var result = SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out string downloadsPath);
        
        return downloadsPath;
    }
}
