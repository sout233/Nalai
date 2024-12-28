using System.IO;
using Microsoft.Win32;
using Nalai.Models;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public class RegManager
{
    private const string FirefoxExtensionId = "nmmnkjnkobkdbjlopninpkddchcbnbkcf";
    private const string ChromeExtensionId = "chrome-extension://nhliacfehfhgideomolhjnnfabmmeiag/";

    public static void RegisterChromeNativeMessagingConfig()
    {
        var config = new NativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
            Path.Combine(Environment.CurrentDirectory, "Nalai.exe"), "stdio", [ChromeExtensionId]);
        var configFilePath = Environment.CurrentDirectory + @"\Configs\org.eu.sout.nalai_chrome.json";

        if (!File.Exists(configFilePath))
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + @"\Configs");
            File.Create(configFilePath).Close();
        }
        
        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        
        var regLoc = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai", true) ??
                     Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai");
        regLoc.SetValue(null, configFilePath);
    }

    public static void RegisterFirefoxNativeMessagingConfig()
    {
        var config = new NativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
            Path.Combine(Environment.CurrentDirectory, "Nalai.exe"), "stdio", [FirefoxExtensionId]);
        var configFilePath = Environment.CurrentDirectory + @"\Configs\org.eu.sout.nalai_firefox.json";

        if (!File.Exists(configFilePath))
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + @"\Configs");
            File.Create(configFilePath).Close();
        }
        
        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        
        var regLoc = Registry.CurrentUser.OpenSubKey(@"Software\Mozilla\NativeMessagingHosts\org.eu.sout.nalai", true) ??
                     Registry.CurrentUser.CreateSubKey(@"Software\Mozilla\NativeMessagingHosts\org.eu.sout.nalai");
        regLoc.SetValue(null, configFilePath);
    }
}