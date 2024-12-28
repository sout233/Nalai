using Nalai.Models;

namespace Nalai.Helpers;

public class RegManager
{
    public static void RegisterNativeMessagingConfig()
    {
        var config = new NativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
            Environment.CurrentDirectory + @"\Nalai.exe", "stdio", ["https://*.sout.eu.org/*"]);
    }
}