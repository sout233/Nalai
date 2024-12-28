using System.IO;
using Microsoft.Win32;
using Nalai.Models;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public class RegManager
{
    public static void RegisterNativeMessagingConfig()
    {
        var config = new NativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
            Environment.CurrentDirectory + @"\Nalai.exe", "stdio", ["https://*.sout.eu.org/*"]);
        var configFilePath = Environment.CurrentDirectory + @"\org.eu.sout.nalai.json";

        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        
        var regLoc = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai", true) ??
                     Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai");
        regLoc.SetValue(null, configFilePath);
        
        regLoc = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Mozilla\NativeMessagingHosts\org.eu.sout.nalai", true) ?? 
                 Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Mozilla\NativeMessagingHosts\org.eu.sout.nalai");
        regLoc.SetValue(null, configFilePath);
    }
}